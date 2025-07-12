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
    public class PerfumesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PerfumesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Perfumes
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PerfumeDto>>> GetPerfumes()
        {
            return await _context.Perfumes
                .Include(p => p.Supplier)
                .Select(p => new PerfumeDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    AvailableQuantity = p.AvailableQuantity,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier != null ? p.Supplier.FullName : "Unknown"
                })
                .ToListAsync();
        }

        // GET: api/Perfumes/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<PerfumeDto>> GetPerfume(int id)
        {
            var perfume = await _context.Perfumes
                .Include(p => p.Supplier)
                .Select(p => new PerfumeDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    AvailableQuantity = p.AvailableQuantity,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier != null ? p.Supplier.FullName : "Unknown"
                })
                .FirstOrDefaultAsync(p => p.Id == id);

            if (perfume == null)
            {
                return NotFound(new { Message = "Perfume not found" });
            }

            return perfume;
        }

        // POST: api/Perfumes
        [HttpPost]
        [Authorize(Roles = "Supplier,Admin")]
        public async Task<ActionResult<Perfume>> CreatePerfume(PerfumeCreateDto perfumeDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var perfume = new Perfume
            {
                Name = perfumeDto.Name,
                Description = perfumeDto.Description,
                Price = perfumeDto.Price,
                AvailableQuantity = perfumeDto.AvailableQuantity,
                SupplierId = User.IsInRole("Supplier") ? userId : perfumeDto.SupplierId
            };

            if (perfume.SupplierId == null)
            {
                return BadRequest("Supplier ID is required");
            }

            _context.Perfumes.Add(perfume);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPerfume), new { id = perfume.Id }, perfume);
        }

        // PUT: api/Perfumes/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Supplier,Admin")]
        public async Task<IActionResult> UpdatePerfume(int id, PerfumeUpdateDto perfumeDto)
        {
            var existingPerfume = await _context.Perfumes.FindAsync(id);
            if (existingPerfume == null)
            {
                return NotFound(new { Message = "Perfume not found" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (User.IsInRole("Supplier") && existingPerfume.SupplierId != userId)
            {
                return Forbid();
            }

            existingPerfume.Name = perfumeDto.Name ?? existingPerfume.Name;
            existingPerfume.Description = perfumeDto.Description ?? existingPerfume.Description;
            existingPerfume.Price = perfumeDto.Price;
            existingPerfume.AvailableQuantity = perfumeDto.AvailableQuantity;

            if (User.IsInRole("Admin") && !string.IsNullOrEmpty(perfumeDto.SupplierId))
            {
                existingPerfume.SupplierId = perfumeDto.SupplierId;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PerfumeExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Perfumes/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Supplier,Admin")]
        public async Task<IActionResult> DeletePerfume(int id)
        {
            var perfume = await _context.Perfumes.FindAsync(id);
            if (perfume == null)
            {
                return NotFound(new { Message = "Perfume not found" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (User.IsInRole("Supplier") && perfume.SupplierId != userId)
            {
                return Forbid();
            }

            _context.Perfumes.Remove(perfume);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PerfumeExists(int id)
        {
            return _context.Perfumes.Any(e => e.Id == id);
        }
    }

    // DTO Classes
    public class PerfumeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int AvailableQuantity { get; set; }
        public string? SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
    }

    public class PerfumeCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int AvailableQuantity { get; set; }
        public string? SupplierId { get; set; }
    }

    public class PerfumeUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int AvailableQuantity { get; set; }
        public string? SupplierId { get; set; }
    }
}