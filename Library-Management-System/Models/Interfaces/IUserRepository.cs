using Library_Management_System.Models;
using System.Threading.Tasks;

namespace Library_Management_System.Models.Interfaces
{
	public interface IUserRepository
	{
		Task<int> GetRegisteredStudentsCountAsync();
		Task<List<StudentDto>> GetAllStudentsAsync();
		Task<List<UserWithRoleDto>> GetAllUsersWithRolesAsync();
	}
}
