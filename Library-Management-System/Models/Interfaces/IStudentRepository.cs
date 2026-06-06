namespace Library_Management_System.Models.Interfaces
{
	public interface IStudentRepository
	{
		IEnumerable<Student> GetAllStudents();
		Student GetStudentById(int id);
		bool CreateStudent(Student student);
		bool UpdateStudent(Student student);
		bool DeleteStudent(int id);
	}
}
