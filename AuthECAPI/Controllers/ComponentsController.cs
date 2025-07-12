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
    public class ComponentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ComponentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Components
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Component>>> GetComponents()
        {
            return await _context.Components
                .Include(c => c.Supplier)
                .Select(c => new Component
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    PricePerUnit = c.PricePerUnit,
                    AvailableQuantity = c.AvailableQuantity,
                    SupplierId = c.SupplierId,
                    Supplier = new AppUser { FullName = c.Supplier.FullName }
                })
                .ToListAsync();
        }

        // GET: api/Components/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Component>> GetComponent(int id)
        {
            var component = await _context.Components
                .Include(c => c.Supplier)
                .Select(c => new Component
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    PricePerUnit = c.PricePerUnit,
                    AvailableQuantity = c.AvailableQuantity,
                    SupplierId = c.SupplierId,
                    Supplier = new AppUser { FullName = c.Supplier.FullName }
                })
                .FirstOrDefaultAsync(c => c.Id == id);

            if (component == null)
            {
                return NotFound();
            }

            return component;
        }

        // POST: api/Components
        [HttpPost]
        [Authorize(Roles = "Supplier,Admin")]
        public async Task<ActionResult<Component>> CreateComponent(Component component)
        {
            // Get the current user id
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // If supplier, set the supplier id to the current user
            if (User.IsInRole("Supplier"))
            {
                component.SupplierId = userId;
            }

            _context.Components.Add(component);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetComponent), new { id = component.Id }, component);
        }

        // PUT: api/Components/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Supplier,Admin")]
        public async Task<IActionResult> UpdateComponent(int id, Component component)
        {
            if (id != component.Id)
            {
                return BadRequest();
            }

            // Get the current user id
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if the component belongs to the current supplier
            var existingComponent = await _context.Components.FindAsync(id);
            if (existingComponent == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Supplier") && existingComponent.SupplierId != userId)
            {
                return Forbid();
            }

            // Only update allowed properties
            existingComponent.Name = component.Name;
            existingComponent.Description = component.Description;
            existingComponent.PricePerUnit = component.PricePerUnit;
            existingComponent.AvailableQuantity = component.AvailableQuantity;

            // If admin and supplier id is changed
            if (User.IsInRole("Admin") && component.SupplierId != null)
            {
                existingComponent.SupplierId = component.SupplierId;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ComponentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Components/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Supplier,Admin")]
        public async Task<IActionResult> DeleteComponent(int id)
        {
            var component = await _context.Components.FindAsync(id);
            if (component == null)
            {
                return NotFound();
            }

            // Get the current user id
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if the component belongs to the current supplier
            if (User.IsInRole("Supplier") && component.SupplierId != userId)
            {
                return Forbid();
            }

            _context.Components.Remove(component);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ComponentExists(int id)
        {
            return _context.Components.Any(e => e.Id == id);
        }
    }
}