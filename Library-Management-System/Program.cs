using Library_Management_System.Data;
using Library_Management_System.Hubs;
using Library_Management_System.Models;
using Library_Management_System.Models.Interfaces;
using Library_Management_System.Models.Repository;
using library_Management_System_01.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Security.Claims;


var builder = WebApplication.CreateBuilder(args);

// -------------------- Services --------------------

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
					   ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
	options.SignIn.RequireConfirmedAccount = true; // Set true if email confirmation is needed
})
.AddEntityFrameworkStores<ApplicationDbContext>();


// Add Controllers with Views
builder.Services.AddControllersWithViews();

// Register repositories
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();

builder.Services.AddScoped<IBorrowRepository, BorrowRepository>();

builder.Services.AddScoped<IUserRepository, UserRepository>();

// Add SignalR
builder.Services.AddSignalR();

// -------------------- Authorization Policies --------------------
builder.Services.AddAuthorization(options =>
{
	// Admin only - using claim
	options.AddPolicy("AdminOnly", policy =>
		policy.RequireClaim("Role", "Admin"));

	// Student only - using claim
	options.AddPolicy("StudentOnly", policy =>
		policy.RequireClaim("Role", "Student"));

	// Admin OR Student
	options.AddPolicy("AdminOrStudent", policy =>
		policy.RequireAssertion(context =>
			context.User.HasClaim(c =>
				c.Type == "Role" &&
				(c.Value == "Admin" || c.Value == "Student"))));
});

var app = builder.Build();

// -------------------- Middleware --------------------
if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
}
else
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


// -------------------- Routes --------------------
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Map SignalR hub
app.MapHub<NotificationHub>("/notificationHub");

// -------------------- Seed Users --------------------

using (var scope = app.Services.CreateScope())
{
	await SeedClaimsData.Initialize(scope.ServiceProvider);
}

app.Run();
