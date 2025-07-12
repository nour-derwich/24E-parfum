using AuthECAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuthECAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Dashboard/Client
        [HttpGet("Client")]
      /*  [Authorize(Roles = "admin ,client , supplier")]*/
        public async Task<IActionResult> GetClientDashboard()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get recent orders
            var recentOrders = await _context.Orders
                .Where(o => o.ClientId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Perfume)
                .ToListAsync();

            // Count orders by status
            var ordersByStatus = await _context.Orders
                .Where(o => o.ClientId == userId)
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return Ok(new
            {
                RecentOrders = recentOrders,
                OrdersByStatus = ordersByStatus
            });
        }

        // GET: api/Dashboard/Supplier
        [HttpGet("Supplier")]
        [Authorize(Roles = "Admin , Client , Supplier")]
        public async Task<IActionResult> GetSupplierDashboard()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get perfumes provided by this supplier
            var perfumes = await _context.Perfumes
                .Where(p => p.SupplierId == userId)
                .ToListAsync();

            // Get components provided by this supplier
            var components = await _context.Components
                .Where(c => c.SupplierId == userId)
                .ToListAsync();

            // Get pending orders for perfumes supplied by this supplier
            var pendingOrders = await _context.Orders
                .Where(o => o.Status == OrderStatus.Pending)
                .Where(o => o.OrderItems.Any(oi => oi.Perfume.SupplierId == userId))
                .Include(o => o.Client)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Perfume)
                .ToListAsync();

            // Get custom orders that use components from this supplier
            var customOrders = await _context.CustomPerfumeOrders
                .Where(cpo => cpo.Components.Any(cpc => cpc.Component.SupplierId == userId))
                .Include(cpo => cpo.Order)
                    .ThenInclude(o => o.Client)
                .Include(cpo => cpo.Components)
                    .ThenInclude(cpc => cpc.Component)
                .ToListAsync();

            return Ok(new
            {
                Perfumes = perfumes,
                Components = components,
                PendingOrders = pendingOrders,
                CustomOrders = customOrders
            });
        }

        // GET: api/Dashboard/Admin
        [HttpGet("Admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            // Count of users by role
            var usersByRole = await _context.Users
                .Cast<AppUser>()
                .GroupBy(u => u.UserRole)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .ToListAsync();

            // Total number of orders
            var totalOrders = await _context.Orders.CountAsync();

            // Orders by status
            var ordersByStatus = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            // Total revenue
            var totalRevenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.TotalPrice);

            // Recent orders
            var recentOrders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Include(o => o.Client)
                .ToListAsync();

            return Ok(new
            {
                UsersByRole = usersByRole,
                TotalOrders = totalOrders,
                OrdersByStatus = ordersByStatus,
                TotalRevenue = totalRevenue,
                RecentOrders = recentOrders
            });
        }
    }
}