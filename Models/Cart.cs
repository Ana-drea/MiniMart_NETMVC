using Microsoft.AspNetCore.Identity;

namespace CapstoneProject.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public Product Product { get; set; } = new Product();

        // Add the UserId field to associate with the user
        public required string UserId { get; set; }  // Corresponds to the Id in the AspNetUsers table
        public IdentityUser? User { get; set; }  // Navigation property
    }
}
