using Microsoft.AspNetCore.Identity;

namespace Library_Management_System.Models
{
	public class ApplicationUser:IdentityUser
	{
		public string? country { get; set; }
	}
}
