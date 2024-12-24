namespace CapstoneProject.ViewModels
{
    public class CartViewModel
    {
        public int Id { get; set; }
        public List<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
        public decimal TotalAmount { get; set; }
        public int TotalQuantity { get; set; }
    }

    public class UpdateQuantityModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

}