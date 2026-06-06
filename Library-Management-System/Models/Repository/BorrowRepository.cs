using Dapper;
using Library_Management_System.Models;
using Library_Management_System.Models.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

public class BorrowRepository : IBorrowRepository
{
	private readonly string _connectionString;

	public BorrowRepository(IConfiguration config)
	{
		_connectionString = config.GetConnectionString("DefaultConnection") ?? "";
	}

	public async Task<bool> CreateBorrowAsync(Borrow borrow)
	{
		using var db = new SqlConnection(_connectionString);
		var sql = @"
            INSERT INTO Borrows 
            (BookId, StudentId, StudentName, Purpose, BorrowDurationDays, Status, RequestDate)
            VALUES 
            (@BookId, @StudentId, @StudentName, @Purpose, @BorrowDurationDays, @Status, @RequestDate)";

		var rows = await db.ExecuteAsync(sql, new
		{
			BookId = borrow.BookId,
			StudentId = borrow.StudentId,
			StudentName = borrow.StudentName,
			Purpose = borrow.Purpose,
			BorrowDurationDays = borrow.BorrowDurationDays,
			Status = borrow.Status,
			RequestDate = borrow.RequestDate
		});

		return rows > 0;
	}

	public async Task<List<Borrow>> GetAllBorrowsAsync()
	{
		using var db = new SqlConnection(_connectionString);
		var sql = "SELECT * FROM Borrows ORDER BY RequestDate DESC";
		var result = await db.QueryAsync<Borrow>(sql);
		return result.ToList();
	}

	public async Task<List<Borrow>> GetBorrowsByStudentIdAsync(string studentId)
	{
		using var db = new SqlConnection(_connectionString);

		var sql = @"
        SELECT 
            b.*,
            bk.*
        FROM Borrows b
        INNER JOIN Books bk ON b.BookId = bk.BookId
        WHERE b.StudentId = @StudentId 
        ORDER BY b.RequestDate DESC";

		// Use dynamic to capture all columns
		var results = await db.QueryAsync(sql, new { StudentId = studentId });

		var borrows = new List<Borrow>();

		foreach (var row in results)
		{
			var borrow = new Borrow
			{
				Id = row.Id,
				BookId = row.BookId,
				StudentId = row.StudentId,
				StudentName = row.StudentName,
				Purpose = row.Purpose,
				BorrowDurationDays = row.BorrowDurationDays,
				Status = row.Status,
				RequestDate = row.RequestDate,
				BorrowDate = row.BorrowDate,
				DueDate = row.DueDate,
				ReturnDate = row.ReturnDate,
				IsReturned = row.IsReturned,
				Book = new Book
				{
					BookId = row.BookId,
					BookName = row.BookName,
					Author = row.Author,
					Genre = row.Genre,
					Description = row.Description,
					PublicationYear = row.PublicationYear,
					Quantity = row.Quantity
				}
			};
			borrows.Add(borrow);
		}

		return borrows;
	}

	public async Task<Borrow?> GetBorrowByIdAsync(int id)
	{
		using var db = new SqlConnection(_connectionString);
		var sql = "SELECT * FROM Borrows WHERE [Id] = @Id";
		return await db.QueryFirstOrDefaultAsync<Borrow>(sql, new { Id = id });
	}

	public async Task<Borrow?> GetBorrowByBookAndStudentAsync(int bookId, string studentId)
	{
		using var db = new SqlConnection(_connectionString);
		var sql = @"
            SELECT * FROM Borrows 
            WHERE BookId = @BookId 
              AND StudentId = @StudentId 
              AND Status IN ('Pending', 'Approved') 
              AND IsReturned = 0";

		return await db.QueryFirstOrDefaultAsync<Borrow>(sql, new
		{
			BookId = bookId,      
			StudentId = studentId 
		});
	}

	public async Task<bool> UpdateBorrowAsync(Borrow borrow)
	{
		using var db = new SqlConnection(_connectionString);
		var sql = @"
            UPDATE Borrows 
            SET Purpose = @Purpose, 
                BorrowDurationDays = @BorrowDurationDays,
                Status = @Status
            WHERE Id = @Id";

		var rows = await db.ExecuteAsync(sql, new
		{
			Id = borrow.Id,                      
			Purpose = borrow.Purpose,
			BorrowDurationDays = borrow.BorrowDurationDays,
			Status = borrow.Status
		});
		return rows > 0;
	}

	public async Task<bool> UpdateStatusAsync(int id, string status)
	{
		using var db = new SqlConnection(_connectionString);
		var sql = "UPDATE Borrows SET Status = @Status WHERE Id = @Id";
		var rows = await db.ExecuteAsync(sql, new { Id = id, Status = status });  
		return rows > 0;
	}

	public async Task<bool> ApproveBorrowAsync(int id, DateTime borrowDate, DateTime dueDate)
	{
		using var db = new SqlConnection(_connectionString);

		
		var bookSql = "SELECT BookId FROM Borrows WHERE Id = @Id";
		var bookId = await db.ExecuteScalarAsync<int>(bookSql, new { Id = id });  

		if (bookId == 0)
			return false;

		var decreaseSql = @"
            UPDATE Books 
            SET Quantity = Quantity - 1 
            WHERE BookId = @BookId AND Quantity > 0";

		var quantityUpdated = await db.ExecuteAsync(decreaseSql, new { BookId = bookId }) > 0;

		if (!quantityUpdated)
			return false;

		
		var approveSql = @"
            UPDATE Borrows 
            SET Status = 'Approved', 
                BorrowDate = @BorrowDate, 
                DueDate = @DueDate 
            WHERE Id = @Id";

		var rows = await db.ExecuteAsync(approveSql, new
		{
			Id = id,          
			BorrowDate = borrowDate,
			DueDate = dueDate
		});
		return rows > 0;
	}

	public async Task<bool> ReturnBorrowAsync(int id)
	{
		using var db = new SqlConnection(_connectionString);

		
		var bookSql = "SELECT BookId FROM Borrows WHERE Id = @Id";
		var bookId = await db.ExecuteScalarAsync<int>(bookSql, new { Id = id });  

		if (bookId == 0)
			return false;

		
		var increaseSql = "UPDATE Books SET Quantity = Quantity + 1 WHERE BookId = @BookId";
		await db.ExecuteAsync(increaseSql, new { BookId = bookId });

		
		var returnSql = @"
            UPDATE Borrows 
            SET Status = 'Returned', 
                ReturnDate = GETDATE(), 
                IsReturned = 1 
            WHERE Id = @Id";

		var rows = await db.ExecuteAsync(returnSql, new { Id = id });  
		return rows > 0;
	}

	public async Task<bool> HasActiveRequestAsync(int bookId, string studentId)
	{
		using var db = new SqlConnection(_connectionString);
		var sql = @"
            SELECT COUNT(*) 
            FROM Borrows 
            WHERE BookId = @BookId 
              AND StudentId = @StudentId 
              AND Status IN ('Pending', 'Approved') 
              AND IsReturned = 0";

		var count = await db.ExecuteScalarAsync<int>(sql, new
		{
			BookId = bookId,      
			StudentId = studentId 
		});

		return count > 0;
	}

	public async Task<int> GetActiveBorrowCountAsync(string studentId)
	{
		using var db = new SqlConnection(_connectionString);
		var sql = @"
            SELECT COUNT(*) 
            FROM Borrows 
            WHERE StudentId = @StudentId 
              AND Status IN ('Pending', 'Approved') 
              AND IsReturned = 0";

		return await db.ExecuteScalarAsync<int>(sql, new { StudentId = studentId });  
	}

	public async Task<int> GetPendingRequestsCountAsync()
	{
		using var db = new SqlConnection(_connectionString);
		var sql = "SELECT COUNT(*) FROM Borrows WHERE Status = 'Pending'";
		return await db.ExecuteScalarAsync<int>(sql);
	}

	public async Task<bool> DecreaseBookQuantityAsync(int bookId)
	{
		using var db = new SqlConnection(_connectionString);
		var sql = @"
            UPDATE Books 
            SET Quantity = Quantity - 1 
            WHERE BookId = @BookId AND Quantity > 0";

		var rows = await db.ExecuteAsync(sql, new { BookId = bookId });  
		return rows > 0;
	}

	public async Task<bool> IncreaseBookQuantityAsync(int bookId)
	{
		using var db = new SqlConnection(_connectionString);
		var sql = "UPDATE Books SET Quantity = Quantity + 1 WHERE BookId = @BookId";
		var rows = await db.ExecuteAsync(sql, new { BookId = bookId });  
		return rows > 0;
	}

	public async Task<IEnumerable<Borrow>> GetOverdueBorrowsAsync()
	{
		using var db = new SqlConnection(_connectionString);

		var sql = @"
        SELECT * 
        FROM Borrows 
        WHERE DueDate < GETDATE() 
          AND IsReturned = 0 
          AND Status = 'Approved'";

		var overdueBorrows = await db.QueryAsync<Borrow>(sql);
		return overdueBorrows;
	}

	public async Task<int> GetActiveBorrowsCountAsync()
	{
		using var db = new SqlConnection(_connectionString);
		var sql = "SELECT COUNT(*) FROM Borrows WHERE Status = 'Approved' AND IsReturned = 0";
		return await db.ExecuteScalarAsync<int>(sql);
	}

	public async Task<int> GetOverdueBorrowsCountAsync()
	{
		using var db = new SqlConnection(_connectionString);

		var sql = @"
            SELECT COUNT(*) 
            FROM Borrows 
            WHERE Status = 'Approved' 
              AND IsReturned = 0 
              AND DueDate < GETDATE()";

		return await db.ExecuteScalarAsync<int>(sql);
	}

	public async Task<bool> MarkAsReturnedAsync(int borrowId, DateTime returnDate)
	{
		using var db = new SqlConnection(_connectionString);

		var sql = @"
            UPDATE Borrows 
            SET IsReturned = 1, 
                ReturnDate = @ReturnDate
            WHERE Id = @BorrowId";

		var affectedRows = await db.ExecuteAsync(sql, new
		{
			BorrowId = borrowId,  
			ReturnDate = returnDate
		});

		return affectedRows > 0;
	}

}