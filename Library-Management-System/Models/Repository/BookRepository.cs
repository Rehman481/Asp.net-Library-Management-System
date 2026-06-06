using Dapper;
using Library_Management_System.Models;
using Library_Management_System.Models.Interfaces;
using Library_Management_System.Models.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;

public class BookRepository : IBookRepository
{
	private readonly IConfiguration _config;

	public BookRepository(IConfiguration config)
	{
		_config = config;
	}

	private IDbConnection CreateConnection() => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

	public async Task<IEnumerable<Book>> GetAllBooksAsync()
	{
		using var connection = CreateConnection();
		const string sql = @"SELECT BookId, BookName, Author, Genre, Description, PublicationYear, Quantity FROM Books";
		return await connection.QueryAsync<Book>(sql);
	}

	public async Task<Book> GetBookByIdAsync(int id)
	{
		using var connection = CreateConnection();
		const string sql = @"SELECT BookId, BookName, Author, Genre, Description, PublicationYear, Quantity 
                             FROM Books WHERE BookId = @Id";
		return await connection.QueryFirstOrDefaultAsync<Book>(sql, new { Id = id });
	}

	public async Task<int> CreateBookAsync(Book book)
	{
		using var connection = CreateConnection();
		const string sql = @"INSERT INTO Books (BookName, Author, Genre, Description, PublicationYear, Quantity) 
                             VALUES (@BookName, @Author, @Genre, @Description, @PublicationYear, @Quantity);
                             SELECT CAST(SCOPE_IDENTITY() as int)";
		return await connection.QuerySingleAsync<int>(sql, book);
	}

	public async Task<bool> UpdateBookAsync(Book book)
	{
		using var connection = CreateConnection();
		const string sql = @"UPDATE Books 
                             SET BookName = @BookName, Author = @Author, Genre = @Genre, Description = @Description, 
                                 PublicationYear = @PublicationYear, Quantity = @Quantity 
                             WHERE BookId = @BookId";
		var rows = await connection.ExecuteAsync(sql, book);
		return rows > 0;
	}

	public async Task<bool> DeleteBookAsync(int id)
	{
		using var connection = CreateConnection();
		const string sql = "DELETE FROM Books WHERE BookId = @Id";
		var rows = await connection.ExecuteAsync(sql, new { Id = id });
		return rows > 0;
	}

	
	public async Task<int> GetTotalBooksAsync()
	{
		using var connection = CreateConnection();
		const string sql = "SELECT COUNT(*) FROM Books";
		return await connection.ExecuteScalarAsync<int>(sql);
	}

	public async Task<int> GetAvailableBooksCountAsync()
	{
		using var connection = CreateConnection();
		const string sql = "SELECT COUNT(*) FROM Books WHERE Quantity > 0";
		return await connection.ExecuteScalarAsync<int>(sql);
	}

	public async Task<int> GetLowStockBooksCountAsync()
	{
		using var connection = CreateConnection();
		const string sql = "SELECT COUNT(*) FROM Books WHERE Quantity > 0 AND Quantity <= 3";
		return await connection.ExecuteScalarAsync<int>(sql);
	}

	public async Task<int> GetOutOfStockBooksCountAsync()
	{
		using var connection = CreateConnection();
		const string sql = "SELECT COUNT(*) FROM Books WHERE Quantity <= 0";
		return await connection.ExecuteScalarAsync<int>(sql);
	}

	public async Task<BookStatistics> GetBookStatisticsAsync()
	{
		using var connection = CreateConnection();
		const string sql = @"
            SELECT 
                COUNT(*) as TotalBooks,
                SUM(CASE WHEN Quantity > 0 THEN 1 ELSE 0 END) as AvailableBooks,
                SUM(CASE WHEN Quantity > 0 AND Quantity <= 3 THEN 1 ELSE 0 END) as LowStockBooks,
                SUM(CASE WHEN Quantity <= 0 THEN 1 ELSE 0 END) as OutOfStockBooks
            FROM Books";

		return await connection.QueryFirstOrDefaultAsync<BookStatistics>(sql)
			?? new BookStatistics();
	}
	

	public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
	{
		using var connection = CreateConnection();
		const string sql = @"
            SELECT BookId, BookName, Author, Genre, Description, PublicationYear, Quantity 
            FROM Books 
            WHERE BookName LIKE @SearchTerm 
               OR Author LIKE @SearchTerm 
               OR Genre LIKE @SearchTerm
               OR Description LIKE @SearchTerm";

		return await connection.QueryAsync<Book>(sql, new { SearchTerm = $"%{searchTerm}%" });
	}

	public async Task<IEnumerable<Book>> GetBooksByGenreAsync(string genre)
	{
		using var connection = CreateConnection();
		const string sql = @"
            SELECT BookId, BookName, Author, Genre, Description, PublicationYear, Quantity 
            FROM Books 
            WHERE Genre = @Genre";

		return await connection.QueryAsync<Book>(sql, new { Genre = genre });
	}

	public async Task<bool> UpdateBookQuantityAsync(int bookId, int newQuantity)
	{
		using var connection = CreateConnection();
		const string sql = "UPDATE Books SET Quantity = @NewQuantity WHERE BookId = @BookId";
		var rows = await connection.ExecuteAsync(sql, new { BookId = bookId, NewQuantity = newQuantity });
		return rows > 0;
	}

	
}