using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.SqlServer;
using CapstoneProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CapstoneProject.Data
{
    public class AppDbContext : IdentityDbContext
    {
        private readonly IConfiguration _configuration;
        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cart> carts { get; set; }
        public DbSet<OrderHistory> orderHistories { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(_configuration.GetConnectionString("Default"), new MySqlServerVersion(new Version(8, 4, 3)));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed Admin User
            SeedAdminUser(modelBuilder);

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            // Configure the foreign key relationship between the Cart table and the AspNetUsers table
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)  // The User property of the Cart class
                .WithMany()  // One user can have multiple cart items
                .HasForeignKey(c => c.UserId)  // Foreign key field
                .OnDelete(DeleteBehavior.Cascade);  // Set cascading delete: cart items will be deleted when the user is deleted

            // Configure the foreign key relationship between the Cart table and the Product table
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Product)  // The Product property of the Cart class
                .WithMany()  // A product can appear in multiple cart items
                .HasForeignKey(c => c.ProductId);  // Foreign key field


            // Category Seed Data
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Fruits", Description = "A variety of fresh fruits" },
                new Category { Id = 2, Name = "Meat", Description = "Fresh cuts of meat" },
                new Category { Id = 3, Name = "Beverages", Description = "Refreshing beverages" }
            );

            // Product Seed Data
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, CategoryId = 1, Name = "Cherry", Description = "Red and sweet cherry", Detail = "Small, round, and juicy cherries, perfect for snacking or baking.", Price = 15.75m, PurchasePrice = 12.75m, QuantityInStock = 10,ImageUrl = "/images/cherry-2369275_1280.jpg" },
                new Product { Id = 2, CategoryId = 1, Name = "Grape", Description = "Fresh and juicy grapes", Detail = "A bunch of sweet and seedless grapes, great for snacks and salads.", Price = 22.1m, PurchasePrice = 12.75m, QuantityInStock = 10,ImageUrl = "/images/grape-23042.jpg"},
                new Product { Id = 3, CategoryId = 1, Name = "Peach", Description = "Sweet and juicy peach", Detail = "A soft and fuzzy peach, bursting with sweet and juicy flavor.", Price = 30.0m, PurchasePrice = 12.75m, QuantityInStock = 10,ImageUrl = "/images/peach-6157041.jpg" },
                new Product { Id = 4, CategoryId = 1, Name = "Pineapple", Description = "Tropical and tangy pineapple", Detail = "A ripe and sweet pineapple, perfect for tropical dishes and drinks.", Price = 9.99m, PurchasePrice = 12.75m, QuantityInStock = 10,ImageUrl = "/images/Pineapple-947879.jpg" },
                new Product { Id = 5, CategoryId = 1, Name = "Watermelon", Description = "Refreshing and hydrating watermelon", Detail = "A large and juicy watermelon, perfect for hot summer days.", Price = 25.0m, PurchasePrice = 12.75m, QuantityInStock = 10,ImageUrl = "/images/Watermelon-1313267.jpg" },
                new Product { Id = 6, CategoryId = 3, Name = "Mineral Water", Description = "Refreshing mineral water", Detail = "A bottle of natural mineral water to keep you hydrated.", Price = 1.99m, PurchasePrice = 12.75m, QuantityInStock = 10,ImageUrl = "/images/water- 27460900.jpg" },
                new Product { Id = 7, CategoryId = 3, Name = "Orange Juice", Description = "Fresh orange juice", Detail = "A bottle of freshly squeezed orange juice, full of vitamin C.", Price = 3.49m, PurchasePrice = 12.75m, QuantityInStock = 10,ImageUrl = "/images/orange juice-5947065.jpg" },
                new Product { Id = 8, CategoryId = 3, Name = "Beer", Description = "Cold and refreshing beer", Detail = "A chilled bottle of beer, perfect for a relaxing evening.", Price = 2.99m, PurchasePrice = 12.75m, QuantityInStock = 10,ImageUrl = "/images/beer-667986.jpg" },
                new Product { Id = 9, CategoryId = 2, Name = "Pork", Description = "Fresh pork", Detail = "A cut of fresh pork, ideal for various dishes.", Price = 8.99m, PurchasePrice = 12.75m, QuantityInStock = 10,ImageUrl = "/images/pork-332784.jpg"  },
                new Product { Id = 10, CategoryId = 2, Name = "Beef", Description = "Tender beef", Detail = "A fresh cut of tender beef, perfect for steaks and roasts.", Price = 12.99m, PurchasePrice = 12.75m, QuantityInStock = 10,ImageUrl = "/images/beef-65175.jpg"  },
                new Product { Id = 11, CategoryId = 2, Name = "Fish", Description = "Fresh fish", Detail = "A fresh fish fillet, ideal for healthy meals.", Price = 9.99m, PurchasePrice = 12.75m, QuantityInStock = 10,ImageUrl = "/images/fish-2792153.jpg" }
            );

            //modelBuilder.Entity<Cart>().HasData(
                // new Cart { Id = 1, ProductId = 1, Quantity = 2 },   // 2 Cherries
                // new Cart { Id = 2, ProductId = 2, Quantity = 1 },   // 1 Grape
                // new Cart { Id = 3, ProductId = 6, Quantity = 5 },   // 5 Mineral Waters
                // new Cart { Id = 4, ProductId = 7, Quantity = 3 },   // 3 Orange Juices
                // new Cart { Id = 5, ProductId = 10, Quantity = 1 }   // 1 Beef
            //);

            modelBuilder.Entity<OrderHistory>().ToTable("orderHistories");
        }

        private void SeedAdminUser(ModelBuilder modelBuilder)
        {
            var adminEmail = "admin@example.com";
            var adminPassword = "AdminPassword123!";

            var adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                NormalizedUserName = adminEmail.ToUpper(),
                NormalizedEmail = adminEmail.ToUpper(),
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            var passwordHasher = new PasswordHasher<IdentityUser>();
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, adminPassword);

            modelBuilder.Entity<IdentityUser>().HasData(adminUser);

            // Use a temporary variable to hold user ID
            var adminUserId = adminUser.Id;

            // Add a claim for the admin user
            modelBuilder.Entity<IdentityUserClaim<string>>().HasData(
                new IdentityUserClaim<string>
                {
                    Id = -1,
                    // Let the database assign an ID automatically
                    UserId = adminUserId,
                    ClaimType = "Position",
                    ClaimValue = "Inventory"
                }
            );
        }
    }
}