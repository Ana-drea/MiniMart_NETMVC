using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Data;
using CapstoneProject.Models;
using CapstoneProject.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CapstoneProject.Controllers;

public class CartController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;  // Inject UserManager

    public CartController(AppDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;  // Assign value
    }

    public async Task<IActionResult> Index()
    {
        // Get the ID of the currently logged-in user
        var user = await _userManager.GetUserAsync(User);  // Assume you use UserManager to get the current user
        var userId = user?.Id;  // Get user ID

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");  // Redirect to the login page if the user is not logged in
        }

        var carts = await _context.carts
            .Where(c => c.UserId == userId)  // Filter out the cart of the current user
            .Include(c => c.Product)
            .ToListAsync();

        var cartViewModel = new CartViewModel
        {
            Products = carts.Select(cart => new ProductViewModel
            {
                Id = cart.ProductId,
                Name = cart.Product.Name,
                Description = cart.Product.Description,
                Price = cart.Product.Price,
                QuantityInCart = cart.Quantity,
                ImageUrl = cart.Product.ImageUrl
            }).ToList(),
            TotalAmount = carts.Sum(cart => cart.Quantity * (decimal)cart.Product.Price),
            TotalQuantity = carts.Sum(cart => cart.Quantity)
        };

        return View(cartViewModel);
    }

    // AddToCart action to handle adding products to the cart
    public async Task<IActionResult> AddToCart(int id, string returnUrl)
    {
        var productName = "";

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");  // Redirect to the login page if the user is not logged in
        }
        var userId = user.Id;  // Get the user's Id

        // Retrieve the product using the id
        var product = await _context.products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        // Check if the cart already contains this product
        var item = await _context.carts.FirstOrDefaultAsync(c => c.ProductId == id && c.UserId == userId);
        if (item == null)
        {
            // If the product is not already in the cart, add a new cart item
            var newItem = new Cart
            {
                ProductId = id,
                Product = product,
                Quantity = 1,
                UserId = userId // Add UserId to the cart item
            };
            productName = product.Name;
            _context.carts.Add(newItem);
        }
        else
        {
            // If the product is already in the cart, increase the quantity
            productName = item.Product.Name;
            item.Quantity++;
            _context.carts.Update(item);
        }

        // Save changes to the database
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = productName + " successfully added to cart";

        // Redirect back to the last page
        Console.WriteLine("returnUrl: " + returnUrl);
        if (!string.IsNullOrEmpty(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }

    public async Task<IActionResult> Remove(int id)
    {
        // Get the ID of the currently logged-in user
        var user = await _userManager.GetUserAsync(User);  // Assume you use UserManager to get the current user
        var userId = user?.Id;  // Get user ID

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");  // Redirect to the login page if the user is not logged in
        }

        var item = await _context.carts.FirstOrDefaultAsync(c => c.ProductId == id && c.UserId == userId);
        if (item == null)
        {
            return NotFound();
        }
        else
        {
            _context.carts.Remove(item);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateQuantity([FromBody] UpdateQuantityModel model)
    {
        // Get the ID of the currently logged-in user
        var user = await _userManager.GetUserAsync(User);  // Assume you use UserManager to get the current user
        if (user == null)
        {
            return RedirectToAction("Login", "Account");  // Redirect to the login page if the user is not logged in
        }
        var userId = user.Id;  // Get user ID

        if (model.ProductId <= 0 || model.Quantity <= 0)
        {
            return BadRequest(new { error = $"Invalid product ID or quantity" });
        }

        var item = await _context.carts
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.ProductId == model.ProductId && c.UserId == userId);

        if (item == null || item.Product == null)
        {
            return NotFound(new { error = "Product not found in cart" });
        }

        // Query the stock quantity from the inventory table
        var stock = await _context.products
            .Where(p => p.Id == model.ProductId)
            .Select(p => p.QuantityInStock)
            .FirstOrDefaultAsync();

        if (stock == 0)
        {
            return NotFound(new { error = "Product is sold out" });
        }

        // Check if stock quantity is sufficient
        if (model.Quantity > stock)
        {
            return BadRequest(new { error = $"Insufficient stock. Maximum available quantity is {stock}" });
        }

        item.Quantity = model.Quantity;
        await _context.SaveChangesAsync();

        decimal subtotal = item.Product.Price * item.Quantity;
        var totalAmount = _context.carts
            .Where(c => c.UserId == userId)
            .Include(c => c.Product)
            .ToList()
            .Sum(c => c.Product.Price * c.Quantity);
        var totalQuantity = _context.carts
            .Where(c => c.UserId == userId)
            .ToList().Sum(c => c.Quantity);

        // Return the updated data
        return Json(new
        {
            subtotal = subtotal,
            totalAmount = totalAmount,
            totalQuantity = totalQuantity
        });
    }
    public async Task<IActionResult> Checkout()
    {
        // Get the ID of the currently logged-in user
        var user = await _userManager.GetUserAsync(User);  // Assume you use UserManager to get the current user
        if (user == null)
        {
            return RedirectToAction("Login", "Account");  // Redirect to the login page if the user is not logged in
        }
        var userId = user.Id;  // Get user ID

        var userName = string.Empty;
        if (user.UserName != null && user.UserName.Length > 0)
        {
            userName = user.UserName.Split('@')[0];
        }

        var orderNumber = GenerateOrderNumber(userName);

        // Get the cart data of the current user
        var cartItems = await _context.carts
            .Where(c => c.UserId == userId)
            .Include(c => c.Product)  // Ensure Product data is loaded
            .ToListAsync();

        if (!cartItems.Any())
        {
            // If the cart is empty, display an error message and cancel checkout
            ViewData["ErrorMessage"] = $"Your cart is empty. Unable to proceed with checkout.";
            return View("BadRequest");
        }

        decimal totalAmount = 0;

        // Create a new OrderHistory instance
        var orderHistory = new OrderHistory
        {
            UserId = userId,
            OrderNumber = orderNumber,
            OrderDate = DateTime.Now,
            OrderItems = new List<OrderItem>()
        };

        // Convert each cart item to OrderItem and add it to OrderHistory
        foreach (var item in cartItems)
        {
            if (item.Product != null) // Ensure Product is not null
            {
                // Query the product inventory
                var productInventory = await _context.products
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                if (productInventory == null || productInventory.QuantityInStock < item.Quantity)
                {
                    // If stock is insufficient, display an error and cancel the order
                    ViewData["ErrorMessage"] = $"Insufficient stock for product '{item.Product.Name}'. Unable to complete the order.";
                    return View("BadRequest");
                }

                // Decrease stock
                productInventory.QuantityInStock -= item.Quantity;

                // Calculate the total price of this item and add it to the order total amount
                decimal itemTotalPrice = item.Quantity * item.Product.Price;
                totalAmount += itemTotalPrice;

                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Product = item.Product,
                    OrderHistoryId = orderHistory.Id,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price,
                    TotalPrice = itemTotalPrice
                };

                orderHistory.OrderItems.Add(orderItem);
            }
        }

        orderHistory.TotalAmount = totalAmount;
        // Save OrderHistory and OrderItems to the database
        _context.orderHistories.Add(orderHistory);
        await _context.SaveChangesAsync();

        // Clear cart data
        _context.carts.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Orders", new { orderId = orderHistory.Id });
    }

    // Order number format: NOXXXXXX-yyyyMMdd, where XXXXXX is a 6-digit random number
    private static string GenerateOrderNumber(string username)
    {
        // Get the current date and time
        string datePart = DateTime.Now.ToString("yyyyMMdd");

        // Generate a six-digit random numeric string
        string randomPart = GenerateRandomCharacters(6);

        // Construct the order number
        string orderNumber = $"NO{randomPart}-{datePart}";

        return orderNumber;
    }

    private static string GenerateRandomCharacters(int length)
    {
        const string chars = "0123456789";
        Random random = new Random();
        char[] result = new char[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = chars[random.Next(chars.Length)];
        }

        return new string(result);
    }

}
