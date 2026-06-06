namespace Library_Management_System.Models
{
	public class UserWithRoleDto
	{
		public string Id { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public bool EmailConfirmed { get; set; }
		public string Roles { get; set; }  // Comma-separated roles

		public bool HasRole(string role) =>
			!string.IsNullOrEmpty(Roles) &&
			Roles.Split(',').Any(r => r.Trim().Equals(role, StringComparison.OrdinalIgnoreCase));

		public bool IsAdmin => HasRole("Admin");
		public bool IsStudent => HasRole("Student") || string.IsNullOrEmpty(Roles);
	}
}
