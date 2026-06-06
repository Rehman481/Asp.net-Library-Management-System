namespace Library_Management_System.Models
{
	public class StudentDto
	{
		public string Id { get; set; }
		public string Username { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public bool EmailConfirmed { get; set; }
		public bool PhoneNumberConfirmed { get; set; }
		public DateTime? LockoutEnd { get; set; }
		public bool LockoutEnabled { get; set; }
		public int AccessFailedCount { get; set; }
		public string RoleName { get; set; }
		// Computed properties
		public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd > DateTime.Now;
		public string Status => IsLockedOut ? "Locked" : "Active";
		public bool IsAdmin => RoleName?.Contains("Admin", StringComparison.OrdinalIgnoreCase) == true;
	}
}
