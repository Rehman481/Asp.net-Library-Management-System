namespace Library_Management_System.Models
{
	public class Notification
	{
		public string Message { get; set; }
		public string Type { get; set; } // info, warning, success, danger
		public string RecipientType { get; set; } // admins, students, specific
		public string? SpecificRecipientId { get; set; }
		public DateTime CreatedAt { get; set; }
		public string CreatedBy { get; set; }
	}
}