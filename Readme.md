### Data File
app.db is an SQLite database file.

### Running the Application
The database already contains data records. You can run the application directly by executing the command dotnet run.

### Commands
The seed data for products and categories is included in Data -> AppDbContext.cs; 
for admin user with inventory authority, in Areas -> Identity -> Data -> AccountContext.cs
You can regenerate it using the following commands:
```bash
dotnet ef migrations list
dotnet ef database drop --context AppDbContext
dotnet ef migrations add InitialCreate --context AppDbContext
dotnet ef migrations remove --context AppDbContext
dotnet ef database update

dotnet ef database update --context AppDbContext
dotnet ef database update --context AccountContext
```

### Register & Login
When register, you need notice "Click here to confirm your account" to confirm your account,
After that you can Succeed Login.


### Admin
user: admin@example.com
pass: AdminPassword123!
Inventory Functions.