using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library_Management_System.Models
{
	
	public class Borrow
	{
		
		public int Id { get; set; }
		public int BookId { get; set; }
		public string StudentId { get; set; } = string.Empty; // GUID from Identity
		public string StudentName { get; set; } = string.Empty;
		public string Purpose { get; set; } = string.Empty;
		public int BorrowDurationDays { get; set; } = 14;
		public string Status { get; set; } = "Pending";
		public DateTime RequestDate { get; set; }
		public DateTime? BorrowDate { get; set; }
		public DateTime? DueDate { get; set; }
		public DateTime? ReturnDate { get; set; }
		public bool IsReturned { get; set; } = false;

		
		public Book? Book { get; set; }


		public string BookName { get; set; }
		public string Author { get; set; }
		public string Genre { get; set; }
		public string Description { get; set; }
		public int? PublicationYear { get; set; }
		public int Quantity { get; set; }

	}
}
