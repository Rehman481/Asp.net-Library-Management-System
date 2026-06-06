using Library_Management_System.Models;

namespace Library_Management_System.Models.Interfaces
{
	public interface IBorrowRepository
	{
		// Create
		
		Task<bool> CreateBorrowAsync(Borrow borrow);
		Task<List<Borrow>> GetAllBorrowsAsync();
		Task<List<Borrow>> GetBorrowsByStudentIdAsync(string studentId);
		Task<Borrow?> GetBorrowByIdAsync(int Id);
		Task<Borrow?> GetBorrowByBookAndStudentAsync(int bookId, string studentId);
		Task<bool> UpdateBorrowAsync(Borrow borrow);
		Task<bool> UpdateStatusAsync(int Id, string status);
		Task<bool> ApproveBorrowAsync(int Id, DateTime borrowDate, DateTime dueDate);
		Task<bool> ReturnBorrowAsync(int Id);
		Task<bool> HasActiveRequestAsync(int bookId, string studentId);
		Task<int> GetActiveBorrowCountAsync(string studentId);
		Task<int> GetPendingRequestsCountAsync();
		Task<IEnumerable<Borrow>> GetOverdueBorrowsAsync();
		Task<bool> DecreaseBookQuantityAsync(int bookId);
		Task<bool> IncreaseBookQuantityAsync(int bookId);
		Task<int> GetActiveBorrowsCountAsync();

		
		Task<int> GetOverdueBorrowsCountAsync();
		
		Task<bool> MarkAsReturnedAsync(int id, DateTime returnDate);
		
		
		
	}
}