using System.ComponentModel.DataAnnotations;

namespace Library_Management_System.Models.ViewModels
{
	public class BorrowViewModel
	{
		[Required]
		public int BookId { get; set; }

		[Required]
		public string BookTitle { get; set; } = string.Empty;

		[Required]
		public string BookAuthor { get; set; } = string.Empty;

		[Required]
		public string StudentName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Please state why you need this book")]
		[StringLength(500, ErrorMessage = "Purpose cannot exceed 500 characters")]
		public string Purpose { get; set; } = string.Empty;

		[Required(ErrorMessage = "Please select borrow duration")]
		[Range(1, 30, ErrorMessage = "Duration must be between 1 and 30 days")]
		public int BorrowDurationDays { get; set; } = 14;

		// For display only
		public int BorrowId { get; set; }
		public string Status { get; set; } = "Pending";
		public DateTime? RequestDate { get; set; }
		public DateTime? DueDate { get; set; }
		public DateTime? BorrowDate { get; set; }
		public DateTime? ReturnDate { get; set; }
		public bool IsReturned { get; set; }

		// For dashboard display
		public int AvailableQuantity { get; set; }
		public int CurrentBorrows { get; set; }
	}
}