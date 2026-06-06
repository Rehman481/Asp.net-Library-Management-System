using Library_Management_System.Hubs;
using Library_Management_System.Models;
using Library_Management_System.Models.Interfaces;
using Library_Management_System.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Library_Management_System.Controllers
{
	[Authorize]
	public class BorrowController : Controller
	{
		private readonly IBorrowRepository _borrowRepo;
		private readonly IBookRepository _bookRepo;
		private readonly IHubContext<NotificationHub> _hubContext;

		public BorrowController(
			IBorrowRepository borrowRepo,
			IBookRepository bookRepo,
			IHubContext<NotificationHub> hubContext)
		{
			_borrowRepo = borrowRepo;
			_bookRepo = bookRepo;
			_hubContext = hubContext;
		}

		// GET: /Borrow/Create?bookId=1
		[HttpGet]
		[Authorize(Policy = "StudentOnly")]
		public async Task<IActionResult> Create(int bookId)
		{
			try
			{
				var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				var studentName = User.Identity?.Name ?? "Student";

				if (string.IsNullOrEmpty(studentId))
				{
					TempData["Error"] = "Please login to continue";
					return RedirectToAction("Login", "Account");
				}

				
				var book = await _bookRepo.GetBookByIdAsync(bookId);
				if (book == null)
				{
					TempData["Error"] = "Book not found";
					return RedirectToAction("Catalog", "Book");
				}

				
				if (book.Quantity <= 0)
				{
					TempData["Error"] = "This book is currently unavailable";
					return RedirectToAction("Catalog", "Book");
				}

			
				var hasActive = await _borrowRepo.HasActiveRequestAsync(bookId, studentId);
				if (hasActive)
				{
					TempData["Error"] = "You already have an active request for this book";
					return RedirectToAction("Catalog", "Book");
				}

				// Check borrow limit
				var activeCount = await _borrowRepo.GetActiveBorrowCountAsync(studentId);
				if (activeCount >= 5)
				{
					TempData["Error"] = "You can only borrow 5 books at a time";
					return RedirectToAction("Catalog", "Book");
				}

				// Prepare view model
				var model = new BorrowViewModel
				{
					BookId = bookId,
					BookTitle = book.BookName,
					BookAuthor = book.Author,
					StudentName = studentName
				};

				ViewBag.BookTitle = book.BookName;
				ViewBag.BookAuthor = book.Author;
				ViewBag.AvailableQuantity = book.Quantity;
				ViewBag.CurrentBorrows = activeCount;

				return View(model);
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"Error: {ex.Message}";
				return RedirectToAction("Catalog", "Book");
			}
		}

		// POST: /Borrow/Create
		[HttpPost]
		[Authorize(Policy = "StudentOnly")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(BorrowViewModel model)
		{
			Console.WriteLine("Create POST method called");

			if (!ModelState.IsValid)
			{
				Console.WriteLine("ModelState is invalid");
				await RepopulateViewData(model.BookId);
				return View(model);
			}

			try
			{
				var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				var studentName = User.Identity?.Name ?? "Student";

				if (string.IsNullOrEmpty(studentId))
				{
					ModelState.AddModelError("", "Please login to continue");
					await RepopulateViewData(model.BookId);
					return View(model);
				}

				// Get book
				var book = await _bookRepo.GetBookByIdAsync(model.BookId);
				if (book == null || book.Quantity <= 0)
				{
					ModelState.AddModelError("", "Book is not available");
					await RepopulateViewData(model.BookId);
					return View(model);
				}

				// Check borrow limit
				var activeCount = await _borrowRepo.GetActiveBorrowCountAsync(studentId);
				if (activeCount >= 5)
				{
					ModelState.AddModelError("", "You can only borrow 5 books at a time");
					await RepopulateViewData(model.BookId);
					return View(model);
				}

				// Check if already has active request
				var hasActive = await _borrowRepo.HasActiveRequestAsync(model.BookId, studentId);
				if (hasActive)
				{
					ModelState.AddModelError("", "You already have an active request for this book");
					await RepopulateViewData(model.BookId);
					return View(model);
				}

				// Create borrow record
				var borrow = new Borrow
				{
					BookId = model.BookId,
					StudentId = studentId,
					StudentName = studentName,
					Purpose = model.Purpose,
					BorrowDurationDays = model.BorrowDurationDays,
					Status = "Pending",
					RequestDate = DateTime.Now
				};

				var success = await _borrowRepo.CreateBorrowAsync(borrow);

				if (success)
				{
					// Send SignalR notification to admins
					await _hubContext.Clients.Group("admins").SendAsync("ReceiveNotification",
						$"📚 New borrow request from {studentName} for '{book.BookName}'");

					// Send notification to student
					await _hubContext.Clients.Group($"user_{studentId}").SendAsync("ReceiveNotification",
						$"✅ Your borrow request for '{book.BookName}' has been submitted!");

					TempData["Success"] = "Borrow request submitted successfully!";

					Console.WriteLine("Redirecting to StudentDashboard...");

					// Handle AJAX vs normal request
					if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
					{
						return Json(new
						{
							success = true,
							message = "Borrow request submitted!",
							redirectUrl = Url.Action("Index", "StudentDashboard")
						});
					}

					// For normal form submission
					return RedirectToAction("Index", "StudentDashboard");
				}
				else
				{
					ModelState.AddModelError("", "Failed to submit borrow request");
					await RepopulateViewData(model.BookId);
					return View(model);
				}
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", $"Error: {ex.Message}");
				await RepopulateViewData(model.BookId);
				return View(model);
			}
		}

		// Helper to repopulate view data
		private async Task RepopulateViewData(int bookId)
		{
			var book = await _bookRepo.GetBookByIdAsync(bookId);
			if (book != null)
			{
				ViewBag.BookTitle = book.BookName;
				ViewBag.BookAuthor = book.Author;
				ViewBag.AvailableQuantity = book.Quantity;

				var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (!string.IsNullOrEmpty(studentId))
				{
					var activeCount = await _borrowRepo.GetActiveBorrowCountAsync(studentId);
					ViewBag.CurrentBorrows = activeCount;
				}
			}
		}

		// GET: /Borrow/MyBorrows
		[HttpGet]
		[Authorize(Policy = "StudentOnly")]
		public async Task<IActionResult> MyBorrows()
		{
			var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrEmpty(studentId))
				return RedirectToAction("Login", "Account");

			var borrows = await _borrowRepo.GetBorrowsByStudentIdAsync(studentId);

			var models = borrows.Select(b => new BorrowViewModel
			{
				BorrowId = b.Id,
				BookId = b.BookId,
				BookTitle = b.Book?.BookName ?? b.BookName ?? "Book",
				BookAuthor = b.Book?.Author ?? b.Author ?? "Unknown",
				StudentName = b.StudentName,
				Purpose = b.Purpose,
				BorrowDurationDays = b.BorrowDurationDays,
				Status = b.Status,
				RequestDate = b.RequestDate,
				BorrowDate = b.BorrowDate,
				DueDate = b.DueDate,
				ReturnDate = b.ReturnDate,
				IsReturned = b.IsReturned
			}).ToList();

			return View(models);
		}

		// GET: /Borrow/Manage (Admin)
		[HttpGet]
		[Authorize(Policy = "AdminOnly")]
		public async Task<IActionResult> Manage()
		{
			var borrows = await _borrowRepo.GetAllBorrowsAsync();

			var models = borrows.Select(b => new BorrowViewModel
			{
				BorrowId = b.Id,
				BookId = b.BookId,
				BookTitle = "Book", // You can load book details here
				StudentName = b.StudentName,
				Purpose = b.Purpose,
				BorrowDurationDays = b.BorrowDurationDays,
				Status = b.Status,
				RequestDate = b.RequestDate,
				DueDate = b.DueDate
			}).ToList();

			return View(models);
		}

		
		[Authorize(Policy = "AdminOnly")]
		public async Task<IActionResult> Approve(int id)
		{
			try
			{
				var borrow = await _borrowRepo.GetBorrowByIdAsync(id);
				if (borrow == null)
				{
					TempData["Error"] = "Borrow request not found";
					return RedirectToAction("Manage");
				}

				
				var book = await _bookRepo.GetBookByIdAsync(borrow.BookId);
				if (book == null || book.Quantity <= 0)
				{
					TempData["Error"] = "Cannot approve: Book is out of stock";
					return RedirectToAction("Manage");
				}

				var borrowDate = DateTime.Now;
				var dueDate = borrowDate.AddDays(borrow.BorrowDurationDays);

				
				var quantityDecreased = await _borrowRepo.DecreaseBookQuantityAsync(borrow.BookId);
				if (!quantityDecreased)
				{
					TempData["Error"] = "Cannot decrease book quantity. Might be out of stock.";
					return RedirectToAction("Manage");
				}

				
				var success = await _borrowRepo.ApproveBorrowAsync(id, borrowDate, dueDate);

				if (success)
				{
					
					await _hubContext.Clients.Group("admins").SendAsync("ReceiveNotification",
						$"✅ Borrow request #{id} approved for {borrow.StudentName}. Quantity decreased.");

					
					await _hubContext.Clients.Group($"user_{borrow.StudentId}").SendAsync("ReceiveNotification",
						$"🎉 Your borrow request has been approved! Book quantity updated.");

					TempData["Success"] = "Borrow request approved! Book quantity decreased.";
				}
				else
				{
					
					await _borrowRepo.IncreaseBookQuantityAsync(borrow.BookId);
					TempData["Error"] = "Failed to approve request";
				}

				return RedirectToAction("Manage");
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"Error: {ex.Message}";
				return RedirectToAction("Manage");
			}
		}

		
		[HttpPost]
		[Authorize(Policy = "AdminOnly")]
		public async Task<IActionResult> Reject(int id)
		{
			try
			{
				var borrow = await _borrowRepo.GetBorrowByIdAsync(id);
				if (borrow == null)
				{
					TempData["Error"] = "Borrow request not found";
					return RedirectToAction("Manage");
				}

				var success = await _borrowRepo.UpdateStatusAsync(id, "Rejected");

				if (success)
				{
					
					await _hubContext.Clients.Group("admins").SendAsync("ReceiveNotification",
						$"❌ Borrow request #{id} rejected for {borrow.StudentName}");

					
					await _hubContext.Clients.Group($"user_{borrow.StudentId}").SendAsync("ReceiveNotification",
						$"⚠️ Your borrow request has been rejected.");

					TempData["Success"] = "Borrow request rejected!";
				}
				else
				{
					TempData["Error"] = "Failed to reject request";
				}

				return RedirectToAction("Manage");
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"Error: {ex.Message}";
				return RedirectToAction("Manage");
			}
		}

		
		[HttpPost]
		[Authorize(Policy = "StudentOnly")]
		public async Task<IActionResult> Return(int id)
		{
			try
			{
				var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				var borrow = await _borrowRepo.GetBorrowByIdAsync(id);

				if (borrow == null || borrow.StudentId != studentId)
				{
					TempData["Error"] = "Cannot return this book";
					return RedirectToAction("MyBorrows");
				}

				
				var quantityIncreased = await _borrowRepo.IncreaseBookQuantityAsync(borrow.BookId);
				if (!quantityIncreased)
				{
					TempData["Error"] = "Failed to update book quantity";
					return RedirectToAction("MyBorrows");
				}

				
				var success = await _borrowRepo.ReturnBorrowAsync(id);

				if (success)
				{
					
					await _hubContext.Clients.Group("admins").SendAsync("ReceiveNotification",
						$"📖 Book returned by {borrow.StudentName}. Quantity increased.");

					TempData["Success"] = "Book returned successfully! Quantity increased.";
				}
				else
				{
					
					await _borrowRepo.DecreaseBookQuantityAsync(borrow.BookId);
					TempData["Error"] = "Failed to return book";
				}

				return RedirectToAction("MyBorrows");
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"Error: {ex.Message}";
				return RedirectToAction("MyBorrows");
			}
		}
	}
}