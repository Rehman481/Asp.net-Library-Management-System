using Library_Management_System.Models;
using Library_Management_System.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Library_Management_System.Controllers
{
	public class StudentController : Controller
	{
		private readonly IStudentRepository _studentRepo;

		public StudentController(IStudentRepository studentRepository)
		{
			_studentRepo = studentRepository;
		}

		public IActionResult Index()
		{
			var students = _studentRepo.GetAllStudents();
			return View(students);
		}

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Create(Student student)
		{
			if (!ModelState.IsValid)
				return View(student);

			var result = _studentRepo.CreateStudent(student);
			if (result)
				return RedirectToAction("Index");

			ModelState.AddModelError("", "Unable to create student.");
			return View(student);
		}

		public IActionResult Edit(int id)
		{
			var student = _studentRepo.GetStudentById(id);
			if (student == null)
				return NotFound();

			return View(student);
		}

		[HttpPost]
		public IActionResult Edit(Student student)
		{
			if (!ModelState.IsValid)
				return View(student);

			var result = _studentRepo.UpdateStudent(student);
			if (result)
				return RedirectToAction("Index");

			ModelState.AddModelError("", "Unable to update student.");
			return View(student);
		}

		public IActionResult Delete(int id)
		{
			var result = _studentRepo.DeleteStudent(id);
			if (!result)
				TempData["Error"] = "Unable to delete student.";

			return RedirectToAction("Index");
		}
	}
}
