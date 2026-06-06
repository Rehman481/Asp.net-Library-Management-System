namespace Library_Management_System.Models.ViewModels
{
	public class StudentDashboardViewModel
	{
		public List<BorrowViewModel> BorrowedBooks { get; set; } = new();
		public List<BorrowViewModel> PendingRequests { get; set; } = new();
		public List<BorrowViewModel> ReturnedBooks { get; set; } = new();
		public int TotalBorrowed => BorrowedBooks.Count;
		public int TotalPending => PendingRequests.Count;
		public int TotalReturned => ReturnedBooks.Count;
	}
}
