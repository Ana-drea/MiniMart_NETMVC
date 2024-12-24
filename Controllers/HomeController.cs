using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Data;
using CapstoneProject.Models;
using CapstoneProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace CapstoneProject.Controllers;
[Authorize]
public class HomeController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;  // Inject UserManager
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, AppDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;  // Assign value
        _logger = logger;
    }

    public async Task<IActionResult> Index(
        int? id,
        string? searchQuery,
        string? sortBy,
        string? sortDirection,
        int pageSize = 5,
        int pageNumber = 1
        )
    {

        // Get the ID of the currently logged-in user
        var user = await _userManager.GetUserAsync(User);  // Assume you use UserManager to get the current user
        var userId = user?.Id;  // Get user ID

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");  // Redirect to the login page if the user is not logged in
        }

        // Searching and filtering
        // If there is a category ID, search for products under that ID
        // Then, if there's a search string, continue searching for product names that contain that string
        var products = new List<Product>();
        products = id.HasValue
            ? await _context.products
                .Where(e => e.CategoryId == id.Value && (string.IsNullOrEmpty(searchQuery) || e.Name.Contains(searchQuery)))
                .ToListAsync()
            : await _context.products
                .Where(e => string.IsNullOrEmpty(searchQuery) || e.Name.Contains(searchQuery))
                .ToListAsync();

        // Sorting
        if (!string.IsNullOrEmpty(sortBy))
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            if (string.Equals(sortBy, "name", StringComparison.OrdinalIgnoreCase))
            {
                products = isDesc ? products.OrderByDescending(e => e.Name).ToList() : products.OrderBy(e => e.Name).ToList();
            }
            else if (string.Equals(sortBy, "price", StringComparison.OrdinalIgnoreCase))
            {
                products = isDesc ? products.OrderByDescending(e => e.Price).ToList() : products.OrderBy(e => e.Price).ToList();
            }
        }

        // Pagination
        var totalRecords = products.Count;
        var totalPages = (int)Math.Ceiling((decimal)totalRecords / pageSize);

        // Prevent pageNumber from being less than 1 or greater than totalPages
        pageNumber = pageNumber < 1 ? 1 : pageNumber > totalPages ? totalPages : pageNumber;

        var skippedResults = (pageNumber - 1) * pageSize;
        products = products.Skip(skippedResults).Take(pageSize).ToList();

        // Create ViewModel and add product IDs related to the current user's cart
        var cartProductIds = await _context.carts
            .Where(c => c.UserId == userId)  // Filter products in the cart by UserId
            .Select(c => c.ProductId)
            .ToListAsync();

        var ViewModel = new ProductCategoryViewModel
        {
            Categories = await _context.categories.ToListAsync(),
            Products = products,
            CartProductIds = cartProductIds
        };

        ViewBag.SelectedCategoryId = id;
        ViewBag.SearchQuery = searchQuery;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDirection = sortDirection;
        ViewBag.TotalRecords = totalRecords;
        ViewBag.TotalPages = totalPages;
        ViewBag.PageNumber = pageNumber;
        return View(ViewModel);
    }

    public async Task<IActionResult> ProductByCategory(int? id)
    {
        IQueryable<Product> query = _context.products;
        if (id != null)
        {
            query = query.Where(e => e.CategoryId == id);
        }
        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.products
                                    .Where(e => e.Id == id)
                                    .SingleOrDefaultAsync();

        if (product == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account"); // If not logged in, redirect to login page
        }
        var userId = user.Id;

        // Check if the item is in the current user's cart
        var isInCart = await _context.carts.AnyAsync(c => c.ProductId == id && c.UserId == userId);

        var ViewModel = new ProductDetailsViewModel
        {
            Product = product,
            IsInCart = isInCart
        };

        return View(ViewModel);
    }
}
