using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapstoneProject.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } =string.Empty;
        public string Description { get; set; }=string.Empty;
        public string Detail { get; set; }=string.Empty;

        [Required]
        [Range(0, int.MaxValue)]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }  // Selling price

        [Required]
        [Range(0, int.MaxValue)]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        public decimal PurchasePrice { get; set; }  // Purchase price
        public string? ImageUrl { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int? QuantityInStock { get; set; }  // New field for inventory quantity
    }
}
