using Dapper;
using Library_Management_System.Models.Interfaces;
using Microsoft.Data.SqlClient;

namespace Library_Management_System.Models.Repository
{
	public class StudentRepository:IStudentRepository
	{
		private readonly string _connectionString;

		public StudentRepository(IConfiguration config)
		{
			_connectionString = config.GetConnectionString("DefaultConnection");
		}

		public IEnumerable<Student> GetAllStudents()
		{
			using var connection = new SqlConnection(_connectionString);
			const string sql = "SELECT * FROM Students";
			return connection.Query<Student>(sql);
		}

		public Student GetStudentById(int id)
		{
			using var connection = new SqlConnection(_connectionString);
			const string sql = "SELECT * FROM Students WHERE StudentId = @Id";
			return connection.QueryFirstOrDefault<Student>(sql, new { Id = id });
		}

		public bool CreateStudent(Student student)
		{
			using var connection = new SqlConnection(_connectionString);
			const string sql = @"
        INSERT INTO Students (StudentName, Email, Phone) 
        VALUES (@StudentName, @Email, @Phone);
        SELECT CAST(SCOPE_IDENTITY() as int)";

			var studentId = connection.ExecuteScalar<int>(sql, student);
			return studentId > 0;
		}

		public bool UpdateStudent(Student student)
		{
			using var connection = new SqlConnection(_connectionString);
			const string sql = @"
                UPDATE Students 
                SET StudentName = @StudentName, Email = @Email, Phone = @Phone 
                WHERE StudentId = @StudentId";

			var affectedRows = connection.Execute(sql, student);
			return affectedRows > 0;
		}

		public bool DeleteStudent(int id)
		{
			using var connection = new SqlConnection(_connectionString);
			const string sql = "DELETE FROM Students WHERE StudentId = @Id";
			var affectedRows = connection.Execute(sql, new { Id = id });
			return affectedRows > 0;
		}

	}
}
