using Library_Management_System.Models.ViewModels;

namespace Library_Management_System.Models.Interfaces
{
	public interface IBookRepository
	{
		Task<IEnumerable<Book>> GetAllBooksAsync();
		Task<Book> GetBookByIdAsync(int id);
		Task<int> CreateBookAsync(Book book);
		Task<bool> UpdateBookAsync(Book book);
		Task<bool> DeleteBookAsync(int id);

		// Statistics Methods
		Task<int> GetTotalBooksAsync();
		Task<int> GetAvailableBooksCountAsync();
		
		Task<int> GetLowStockBooksCountAsync();
		Task<int> GetOutOfStockBooksCountAsync();
		Task<BookStatistics> GetBookStatisticsAsync();

		// Search/Filter Methods
		Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm);
		Task<IEnumerable<Book>> GetBooksByGenreAsync(string genre);

		// Quantity Management
		Task<bool> UpdateBookQuantityAsync(int bookId, int newQuantity);
		
	}
}
