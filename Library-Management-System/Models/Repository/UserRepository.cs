using Dapper;
using Library_Management_System.Models.Interfaces;
using Library_Management_System.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Library_Management_System.Models.Repository
{
	public class UserRepository : IUserRepository
	{
		private readonly IConfiguration _config;

		public UserRepository(IConfiguration config)
		{
			_config = config;
		}

		private IDbConnection CreateConnection() => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

		public async Task<int> GetRegisteredStudentsCountAsync()
		{
			using var connection = CreateConnection();
			const string sql = "SELECT COUNT(*) FROM AspNetUsers";
			return await connection.ExecuteScalarAsync<int>(sql);
		}

		public async Task<List<StudentDto>> GetAllStudentsAsync()
		{
			using var connection = CreateConnection();
			const string sql = @"
                SELECT 
                    u.Id,
                    u.UserName as Username,
                    u.Email,
                    u.PhoneNumber as Phone,
                    u.EmailConfirmed,
                    u.PhoneNumberConfirmed,
                    u.LockoutEnd,
                    u.LockoutEnabled,
                    u.AccessFailedCount,
                    r.Name as RoleName,  -- Added role info
                    u.TwoFactorEnabled,
                    u.NormalizedUserName,
                    u.NormalizedEmail,
                    u.SecurityStamp,
                    u.ConcurrencyStamp
                FROM AspNetUsers u
                LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
                LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
                ORDER BY u.UserName";

			var users = await connection.QueryAsync<StudentDto>(sql);
			return users.ToList();
		}

		// NEW METHOD: Get users with their roles
		public async Task<List<UserWithRoleDto>> GetAllUsersWithRolesAsync()
		{
			using var connection = CreateConnection();
			const string sql = @"
                SELECT 
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.PhoneNumber,
                    u.EmailConfirmed,
                    STRING_AGG(r.Name, ', ') as Roles  -- Group multiple roles
                FROM AspNetUsers u
                LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
                LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
                GROUP BY u.Id, u.UserName, u.Email, u.PhoneNumber, u.EmailConfirmed
                ORDER BY u.UserName";

			return (await connection.QueryAsync<UserWithRoleDto>(sql)).ToList();
		}
	}
}