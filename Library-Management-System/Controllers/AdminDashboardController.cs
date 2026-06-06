using Library_Management_System.Hubs;
using Library_Management_System.Models;
using Library_Management_System.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Library_Management_System.Controllers
{
	[Authorize(Policy = "AdminOnly")]
	public class AdminDashboardController : Controller
	{
		private readonly IBorrowRepository _borrowRepo;
		private readonly IBookRepository _bookRepo;
		private readonly IUserRepository _userRepo;
		private readonly IHubContext<NotificationHub> _hubContext;
		

		public AdminDashboardController(
			IBorrowRepository borrowRepo,
			IBookRepository bookRepo,
			IUserRepository userRepo,
			IHubContext<NotificationHub> hubContext
			)
		{
			_borrowRepo = borrowRepo;
			_bookRepo = bookRepo;
			_userRepo = userRepo;
			_hubContext = hubContext;
			
		}

		public async Task<IActionResult> Index()
		{
			var adminName = User.Identity?.Name ?? "Admin";

			// Get all statistics
			var allBorrows = await _borrowRepo.GetAllBorrowsAsync();
			var allBooks = await _bookRepo.GetAllBooksAsync();
			var registeredStudents = await _userRepo.GetRegisteredStudentsCountAsync();

			// Borrow statistics
			var pendingRequests = allBorrows.Count(b => b.Status == "Pending");
			var approvedBorrows = allBorrows.Count(b => b.Status == "Approved" && !b.IsReturned);
			var returnedBooks = allBorrows.Count(b => b.IsReturned);
			var totalBorrows = allBorrows.Count;

			// Book statistics
			var bookStats = await _bookRepo.GetBookStatisticsAsync();
			var totalBooks = bookStats.TotalBooks;
			var availableBooks = bookStats.AvailableBooks;
			var lowStockBooks = bookStats.LowStockBooks;
			var outOfStockBooks = bookStats.OutOfStockBooks;

			// Fine statistics
			

			// Recent activity
			var recentBorrows = allBorrows.OrderByDescending(b => b.RequestDate).Take(10).ToList();
			var recentPending = allBorrows.Where(b => b.Status == "Pending")
										.OrderByDescending(b => b.RequestDate)
										.Take(5).ToList();

			ViewBag.AdminName = adminName;
			ViewBag.PendingRequests = pendingRequests;
			ViewBag.ApprovedBorrows = approvedBorrows;
			ViewBag.ReturnedBooks = returnedBooks;
			ViewBag.TotalBorrows = totalBorrows;
			ViewBag.TotalBooks = totalBooks;
			ViewBag.AvailableBooks = availableBooks;
			ViewBag.LowStockBooks = lowStockBooks;
			ViewBag.OutOfStockBooks = outOfStockBooks;
			ViewBag.RegisteredStudents = registeredStudents;
			ViewBag.RecentBorrows = recentBorrows;
			ViewBag.RecentPending = recentPending;
			
			return View();
		}

		// Quick Actions
		[HttpPost]
		public async Task<IActionResult> QuickApprove(int id)
		{
			var borrow = await _borrowRepo.GetBorrowByIdAsync(id);
			if (borrow == null)
			{
				TempData["Error"] = "Borrow request not found";
				return RedirectToAction("Index");
			}

			// Check if book still has quantity
			var book = await _bookRepo.GetBookByIdAsync(borrow.BookId);
			if (book == null || book.Quantity <= 0)
			{
				TempData["Error"] = "Cannot approve: Book is out of stock";
				return RedirectToAction("Index");
			}

			var borrowDate = DateTime.Now;
			var dueDate = borrowDate.AddDays(borrow.BorrowDurationDays);

			// Decrease quantity
			var quantityDecreased = await _borrowRepo.DecreaseBookQuantityAsync(borrow.BookId);
			if (!quantityDecreased)
			{
				TempData["Error"] = "Cannot decrease book quantity";
				return RedirectToAction("Index");
			}

			var success = await _borrowRepo.ApproveBorrowAsync(id, borrowDate, dueDate);

			if (success)
			{
				await _hubContext.Clients.Group("admins").SendAsync("ReceiveNotification",
					$"✅ Quick Approve: Request #{id} approved for {borrow.StudentName}");

				await _hubContext.Clients.Group($"user_{borrow.StudentId}").SendAsync("ReceiveNotification",
					$"🎉 Your borrow request has been approved!");

				TempData["Success"] = $"Borrow request #{id} approved!";
			}
			else
			{
				await _borrowRepo.IncreaseBookQuantityAsync(borrow.BookId);
				TempData["Error"] = "Failed to approve request";
			}

			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> QuickReject(int id)
		{
			var borrow = await _borrowRepo.GetBorrowByIdAsync(id);
			if (borrow == null)
			{
				TempData["Error"] = "Borrow request not found";
				return RedirectToAction("Index");
			}

			var success = await _borrowRepo.UpdateStatusAsync(id, "Rejected");

			if (success)
			{
				await _hubContext.Clients.Group("admins").SendAsync("ReceiveNotification",
					$"❌ Quick Reject: Request #{id} rejected for {borrow.StudentName}");

				await _hubContext.Clients.Group($"user_{borrow.StudentId}").SendAsync("ReceiveNotification",
					$"⚠️ Your borrow request has been rejected.");

				TempData["Success"] = $"Borrow request #{id} rejected!";
			}
			else
			{
				TempData["Error"] = "Failed to reject request";
			}

			return RedirectToAction("Index");
		}

		// View all pending requests
		public async Task<IActionResult> PendingRequests()
		{
			var borrows = await _borrowRepo.GetAllBorrowsAsync();
			var pending = borrows.Where(b => b.Status == "Pending")
							   .OrderByDescending(b => b.RequestDate)
							   .ToList();

			return View(pending);
		}

		// View all active borrows
		public async Task<IActionResult> ActiveBorrows()
		{
			var borrows = await _borrowRepo.GetAllBorrowsAsync();
			var active = borrows.Where(b => b.Status == "Approved" && !b.IsReturned)
							  .OrderByDescending(b => b.BorrowDate)
							  .ToList();

			return View(active);
		}

		// View overdue books with fines
		public async Task<IActionResult> OverdueBooks()
		{
			// Get the list of overdue borrows
			var overdueBorrows = await _borrowRepo.GetOverdueBorrowsAsync();

			// Get count from the list
			ViewBag.OverdueCount = overdueBorrows.Count();

			// Return view with the list
			return View(overdueBorrows.ToList());
		}

		// System notifications
		public IActionResult Notifications()
		{
			return View();
		}

		// Send notification to all users
		[HttpPost]
		public async Task<IActionResult> SendNotification(string message, string notificationType = "info")
		{
			if (string.IsNullOrEmpty(message))
			{
				TempData["Error"] = "Message cannot be empty";
				return RedirectToAction("Notifications");
			}

			// Send to all admins
			await _hubContext.Clients.Group("admins").SendAsync("ReceiveNotification",
				$"📢 Admin Broadcast: {message}");

			TempData["Success"] = "Notification sent to all admins!";
			return RedirectToAction("Notifications");
		}

		// View all registered users/students
		public async Task<IActionResult> RegisteredStudents()
		{
			var students = await _userRepo.GetAllStudentsAsync();

			

			return View(students);
		}

		

		
		
			


		// DASHBOARD STATS API (for AJAX updates)
		[HttpGet]
		public async Task<IActionResult> GetDashboardStats()
		{
			var stats = new
			{
				PendingRequests = await _borrowRepo.GetPendingRequestsCountAsync(),
				ActiveBorrows = await _borrowRepo.GetActiveBorrowsCountAsync(),
				OverdueCount = await _borrowRepo.GetOverdueBorrowsCountAsync(),
				
				RegisteredStudents = await _userRepo.GetRegisteredStudentsCountAsync(),
				AvailableBooks = (await _bookRepo.GetBookStatisticsAsync()).AvailableBooks
			};

			return Json(stats);
		}
		
				

			
		
				
		
		
	}
}