using CapstoneProject.Models;
namespace CapstoneProject.ViewModels
{
    public class ProductCategoryViewModel
    {
        public IEnumerable<Category>? Categories { get; set; }
        public IEnumerable<Product>? Products { get; set; }
        public List<int>? CartProductIds { get; set; }
    }
}
