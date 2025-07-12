// Modified AppUser class
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthECAPI.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty; // Initialisation
        public string UserRole { get; set; } = "Client";

        // Propriétés de navigation manquantes
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Perfume> SuppliedPerfumes { get; set; } = new List<Perfume>();
        public ICollection<Component> SuppliedComponents { get; set; } = new List<Component>();
    }

    // Enum for order status
    public enum OrderStatus
    {
        Pending,
        InProduction,
        Delivered
    }

    // Model for perfumes catalog
    public class Perfume
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int AvailableQuantity { get; set; }
        public string SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public AppUser Supplier { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }

    // Model for order
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public decimal TotalPrice { get; set; }
        public string ClientId { get; set; }

        [ForeignKey("ClientId")]
        public AppUser Client { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
        public bool IsCustomOrder { get; set; } = false;
    }

    // Model for order items (relates orders to perfumes with quantity)
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int? PerfumeId { get; set; }
        public Perfume Perfume { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    // Models for custom perfume orders
    public class Component
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal PricePerUnit { get; set; }
        public int AvailableQuantity { get; set; }
        public string SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public AppUser Supplier { get; set; }

        public ICollection<CustomPerfumeComponent> CustomPerfumeComponents { get; set; }
    }

    public class CustomPerfumeOrder
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public decimal Price { get; set; }
        public string Notes { get; set; }

        public ICollection<CustomPerfumeComponent> Components { get; set; }
    }

    public class CustomPerfumeComponent
    {
        public int Id { get; set; }
        public int CustomPerfumeOrderId { get; set; }
        public CustomPerfumeOrder CustomPerfumeOrder { get; set; }
        public int ComponentId { get; set; }
        public Component Component { get; set; }
        public int Quantity { get; set; }
    }
}