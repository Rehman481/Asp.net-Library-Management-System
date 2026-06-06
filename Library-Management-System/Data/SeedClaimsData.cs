using Library_Management_System.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace library_Management_System_01.Data
{
	public class SeedClaimsData
	{
		public static async Task Initialize(IServiceProvider serviceProvider)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

				// Create Admin user
				var adminEmail = "admin@library.com";
				var adminUser = await userManager.FindByEmailAsync(adminEmail);

				if (adminUser == null)
				{
					adminUser = new ApplicationUser
					{
						UserName = adminEmail,
						Email = adminEmail,
						country = "United States", // Only custom field
						EmailConfirmed = true
					};
					await userManager.CreateAsync(adminUser, "Admin@123");
					await userManager.AddClaimAsync(adminUser, new Claim("Role", "Admin"));
					
				}

				// Create Student user
				var studentEmail = "student@library.com";
				var studentUser = await userManager.FindByEmailAsync(studentEmail);

				if (studentUser == null)
				{
					studentUser = new ApplicationUser
					{
						UserName = studentEmail,
						Email = studentEmail,
						country = "United States", // Only custom field
						EmailConfirmed = true
					};
					await userManager.CreateAsync(studentUser, "Student@123");
					await userManager.AddClaimAsync(studentUser, new Claim("Role", "Student"));
					
				}
				var newStudentEmail = "robert@example.com";
				var newStudentUser = await userManager.FindByEmailAsync(newStudentEmail);

				if (newStudentUser == null)
				{
					newStudentUser = new ApplicationUser
					{
						UserName = newStudentEmail,
						Email = newStudentEmail,
						country = "United States", 
						EmailConfirmed = true
					};
					await userManager.CreateAsync(newStudentUser, "Robert@123"); 
					await userManager.AddClaimAsync(newStudentUser, new Claim("Role", "Student"));
				}
			}
		}
	}
}