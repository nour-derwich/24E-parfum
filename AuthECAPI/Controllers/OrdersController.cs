using AuthECAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuthECAPI.Controllers
{
    // DTOs for Orders
    public class CreateOrderDto
    {
        public List<OrderItemDto> OrderItems { get; set; }
    }

    public class OrderItemDto
    {
        public int PerfumeId { get; set; }
        public int Quantity { get; set; }
    }

    public class CreateCustomOrderDto
    {
        public List<CustomComponentDto> Components { get; set; }
        public string Notes { get; set; }
    }

    public class CustomComponentDto
    {
        public int ComponentId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        public OrderStatus Status { get; set; }
        public decimal? Price { get; set; } // For custom orders
    }

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            // Get current user id and role
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole("Admin");
            bool isSupplier = User.IsInRole("Supplier");

            var query = _context.Orders
                .Include(o => o.Client)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Perfume)
                .AsQueryable();

            // Filter based on role
            if (!isAdmin)
            {
                if (isSupplier)
                {
                    // Suppliers see orders with perfumes they supply
                    query = query.Where(o => o.OrderItems.Any(oi => oi.Perfume.SupplierId == userId));
                }
                else
                {
                    // Clients see only their own orders
                    query = query.Where(o => o.ClientId == userId);
                }
            }

            return await query.ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            // Get current user id and role
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole("Admin");
            bool isSupplier = User.IsInRole("Supplier");

            var order = await _context.Orders
                .Include(o => o.Client)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Perfume)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Check permissions
            if (!isAdmin && !isSupplier && order.ClientId != userId)
            {
                return Forbid();
            }

            if (isSupplier && !order.OrderItems.Any(oi => oi.Perfume.SupplierId == userId))
            {
                return Forbid();
            }

            return order;
        }

        // POST: api/Orders
        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<ActionResult<Order>> CreateOrder(CreateOrderDto createOrderDto)
        {
            // Get current user id
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Begin transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create new order
                var order = new Order
                {
                    ClientId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    IsCustomOrder = false,
                    OrderItems = new List<OrderItem>()
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                decimal totalPrice = 0;

                // Add order items
                foreach (var item in createOrderDto.OrderItems)
                {
                    // Get perfume details
                    var perfume = await _context.Perfumes.FindAsync(item.PerfumeId);
                    if (perfume == null)
                    {
                        throw new Exception($"Perfume with id {item.PerfumeId} not found");
                    }

                    // Check if enough quantity available
                    if (perfume.AvailableQuantity < item.Quantity)
                    {
                        throw new Exception($"Not enough stock for perfume {perfume.Name}");
                    }

                    // Create order item
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        PerfumeId = item.PerfumeId,
                        Quantity = item.Quantity,
                        UnitPrice = perfume.Price
                    };

                    _context.OrderItems.Add(orderItem);

                    // Update total price
                    totalPrice += perfume.Price * item.Quantity;

                    // Update stock
                    perfume.AvailableQuantity -= item.Quantity;
                    _context.Entry(perfume).State = EntityState.Modified;
                }

                // Update order total price
                order.TotalPrice = totalPrice;
                _context.Entry(order).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/Orders/Custom
        [HttpPost("Custom")]
        [Authorize(Roles = "Client")]
        public async Task<ActionResult<Order>> CreateCustomOrder(CreateCustomOrderDto createCustomOrderDto)
        {
            // Get current user id
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Begin transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create new order
                var order = new Order
                {
                    ClientId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    IsCustomOrder = true,
                    TotalPrice = 0 // Price will be set by supplier later
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Create custom perfume order
                var customOrder = new CustomPerfumeOrder
                {
                    OrderId = order.Id,
                    Notes = createCustomOrderDto.Notes,
                    Components = new List<CustomPerfumeComponent>()
                };

                _context.CustomPerfumeOrders.Add(customOrder);
                await _context.SaveChangesAsync();

                // Add components
                foreach (var componentDto in createCustomOrderDto.Components)
                {
                    // Get component details
                    var component = await _context.Components.FindAsync(componentDto.ComponentId);
                    if (component == null)
                    {
                        throw new Exception($"Component with id {componentDto.ComponentId} not found");
                    }

                    // Check if enough quantity available
                    if (component.AvailableQuantity < componentDto.Quantity)
                    {
                        throw new Exception($"Not enough stock for component {component.Name}");
                    }

                    // Create custom component
                    var customComponent = new CustomPerfumeComponent
                    {
                        CustomPerfumeOrderId = customOrder.Id,
                        ComponentId = componentDto.ComponentId,
                        Quantity = componentDto.Quantity
                    };

                    _context.CustomPerfumeComponents.Add(customComponent);

                    // Update stock
                    component.AvailableQuantity -= componentDto.Quantity;
                    _context.Entry(component).State = EntityState.Modified;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Orders/5/Status
        [HttpPut("{id}/Status")]
        [Authorize(Roles = "Supplier,Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int id, UpdateOrderStatusDto updateStatusDto)
        {
            // Get current user id and role
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole("Admin");
            bool isSupplier = User.IsInRole("Supplier");

            // Begin transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get order
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Perfume)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound();
                }

                // Check permissions for supplier
                if (isSupplier && !order.OrderItems.Any(oi => oi.Perfume.SupplierId == userId))
                {
                    return Forbid();
                }

                // Update status
                order.Status = updateStatusDto.Status;

                // If it's a custom order and price is provided, update price
                if (order.IsCustomOrder && updateStatusDto.Price.HasValue)
                {
                    // Get custom order
                    var customOrder = await _context.CustomPerfumeOrders
                        .FirstOrDefaultAsync(cpo => cpo.OrderId == id);

                    if (customOrder != null)
                    {
                        customOrder.Price = updateStatusDto.Price.Value;
                        order.TotalPrice = updateStatusDto.Price.Value;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Orders/Custom/5
        [HttpGet("Custom/{id}")]
        public async Task<ActionResult<CustomPerfumeOrder>> GetCustomOrder(int id)
        {
            // Get current user id and role
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole("Admin");
            bool isSupplier = User.IsInRole("Supplier");

            var customOrder = await _context.CustomPerfumeOrders
                .Include(cpo => cpo.Order)
                .Include(cpo => cpo.Components)
                    .ThenInclude(cpc => cpc.Component)
                .FirstOrDefaultAsync(cpo => cpo.OrderId == id);

            if (customOrder == null)
            {
                return NotFound();
            }

            // Check permissions
            if (!isAdmin && !isSupplier && customOrder.Order.ClientId != userId)
            {
                return Forbid();
            }

            return customOrder;
        }
    }
}