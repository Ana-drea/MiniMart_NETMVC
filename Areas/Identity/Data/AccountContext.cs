using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CapstoneProject.Data;

public class AccountContext : IdentityDbContext<IdentityUser>
{
    public AccountContext(DbContextOptions<AccountContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed Admin User
        SeedAdminUser(modelBuilder);
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
