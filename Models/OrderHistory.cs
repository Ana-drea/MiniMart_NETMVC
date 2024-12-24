using Microsoft.AspNetCore.Identity;

namespace CapstoneProject.Models
{
    public class OrderHistory
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public required string UserId { get; set; }  // Foreign key, linked to the Id in the AspNetUsers table
        public IdentityUser? User { get; set; }  // Navigation property
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>{};  // Linked to the items in each order
    }

    // Table used to record each order item
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderHistoryId { get; set; }
        public OrderHistory? OrderHistory { get; set; }  // Linked to the order history table
        public int ProductId { get; set; }
        public Product Product { get; set; } = new Product();
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }  // Records the unit price of the product
        public decimal TotalPrice { get; set; }  // Records the total price of the product
    }
}
