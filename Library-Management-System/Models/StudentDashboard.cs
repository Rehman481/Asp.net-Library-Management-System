namespace Library_Management_System.Models
{
	public class StudentDashboard
	{
		public IEnumerable<Borrow> BorrowedBooks { get; set; }
		public IEnumerable<Borrow> PendingRequests { get; set; }
		public IEnumerable<Borrow> ReturnedBooks { get; set; }

		public int TotalBorrowed { get; set; }
		public int TotalPending { get; set; }
		public int TotalReturned { get; set; }
	}
}
