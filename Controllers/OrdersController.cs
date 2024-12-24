using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Data;
using CapstoneProject.Models;
using CapstoneProject.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapstoneProject.Controllers;

public class OrdersController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public OrdersController(AppDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var userId = user?.Id;

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var orders = await _context.orderHistories
                                    .Include(o => o.OrderItems)
                                    .ThenInclude(oi => oi.Product) // Assume the Product navigation property of OrderItem is already loaded
                                    .Where(o => o.UserId == userId)
                                    .OrderByDescending(o => o.OrderDate)  // Sort by OrderDate in descending order
                                    .ToListAsync();

        var orderNames = orders
            .Select(o => new SelectListItem
            {
                Text = o.OrderNumber,
                Value = o.Id.ToString()
            }).ToList();

        ViewBag.OrderNames = orderNames;

        // If there are orders, get the latest order
        var latestOrder = orders.FirstOrDefault();
        if (latestOrder != null)
        {
            // Return the details view of the latest order
            return View(latestOrder);
        }

        return View(); // Return an empty view if there are no orders
    }

    [HttpPost]
    public async Task<IActionResult> Index(int orderHistory)
    {
        if (orderHistory == 0)
        {
            return RedirectToAction("Index"); // Redirect if no order is selected
        }

        var user = await _userManager.GetUserAsync(User);
        var userId = user?.Id;

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var orders = await _context.orderHistories
                            .Where(o => o.UserId == userId)
                            .ToListAsync();

        var orderNames = orders
            .OrderByDescending(o => o.OrderDate)  // Sort by OrderDate in descending order
            .Select(o => new SelectListItem
            {
                Text = o.OrderNumber,
                Value = o.Id.ToString()
            }).ToList();

        ViewBag.OrderNames = orderNames;

        // Query the details of the order by order ID
        var order = await _context.orderHistories
                                    .Include(o => o.OrderItems)
                                    .ThenInclude(oi => oi.Product) // Assume the Product navigation property of OrderItem is already loaded
                                    .FirstOrDefaultAsync(o => o.Id == orderHistory && o.UserId == userId);

        if (order == null)
        {
            return RedirectToAction("Index"); // Redirect if the order is not found
        }

        return View(order); // Return the order details view
    }

    [HttpGet]
    public async Task<IActionResult> AdminOrders()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var claims = await _userManager.GetClaimsAsync(user);
        var isAdmin = claims.Any(c => c.Type == "Position" && c.Value == "Inventory");

        if (!isAdmin)
        {
            return Forbid();
        }

        var orders = await _context.orderHistories
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ToListAsync();

        return View("AdminOrders", orders);
    }

}
