using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;


namespace AuthECAPI.Models
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }

        public DbSet<Perfume> Perfumes { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Component> Components { get; set; }
        public DbSet<CustomPerfumeOrder> CustomPerfumeOrders { get; set; }
        public DbSet<CustomPerfumeComponent> CustomPerfumeComponents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration des tables Identity
            modelBuilder.Entity<AppUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            // ... autres configurations de table

            // Configuration des relations
            modelBuilder.Entity<AppUser>(b =>
            {
                b.HasMany(u => u.Orders)
                    .WithOne(o => o.Client)
                    .HasForeignKey(o => o.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(u => u.SuppliedPerfumes)
                    .WithOne(p => p.Supplier)
                    .HasForeignKey(p => p.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(u => u.SuppliedComponents)
                    .WithOne(c => c.Supplier)
                    .HasForeignKey(c => c.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuration des types décimales
            modelBuilder.Entity<Perfume>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Component>()
                .Property(c => c.PricePerUnit)
                .HasPrecision(18, 2);

            // Configuration des index
            modelBuilder.Entity<Perfume>()
                .HasIndex(p => new { p.Name, p.SupplierId });

            modelBuilder.Entity<Component>()
                .HasIndex(c => new { c.Name, c.SupplierId });
        }
    }
}