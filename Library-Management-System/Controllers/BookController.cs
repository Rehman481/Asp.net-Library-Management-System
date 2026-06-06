using Library_Management_System.Models;
using Library_Management_System.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library_Management_System.Controllers
{
	
	public class BookController : Controller
	{
		private readonly IBookRepository _bookRepo;

		public BookController(IBookRepository bookRepository) => _bookRepo = bookRepository;
		[Authorize(Policy = "AdminOnly")]
		public async Task<IActionResult> Index()
		{
			var books = await _bookRepo.GetAllBooksAsync();
			return View(books);
		}
		[Authorize(Policy = "AdminOrStudent")]
		public IActionResult Catalog() => View(_bookRepo.GetAllBooksAsync().Result); 

		[HttpGet]
		public async Task<IActionResult> Details(int id)
		{
			var book = await _bookRepo.GetBookByIdAsync(id);
			if (book == null) return NotFound();
			return View(book);
		}

		[Authorize(Policy = "AdminOnly")]
		public IActionResult Create() => View();

		[HttpPost]
		[Authorize(Policy = "AdminOnly")]
		public async Task<IActionResult> Create(Book book)
		{
			if (!ModelState.IsValid) return View(book);
			await _bookRepo.CreateBookAsync(book);
			return RedirectToAction("Index");
		}

		[Authorize(Policy = "AdminOnly")]
		public async Task<IActionResult> Edit(int id)
		{
			var book = await _bookRepo.GetBookByIdAsync(id);
			if (book == null) return NotFound();
			return View(book);
		}

		[HttpPost]
		[Authorize(Policy = "AdminOnly")]
		public async Task<IActionResult> Edit(Book book)
		{
			if (!ModelState.IsValid) return View(book);
			await _bookRepo.UpdateBookAsync(book);
			return RedirectToAction("Index");
		}

		[Authorize(Policy = "AdminOnly")]
		public async Task<IActionResult> Delete(int id)
		{
			await _bookRepo.DeleteBookAsync(id);
			return RedirectToAction("Index");
		}
	}
}

