using Library_Management_System.Hubs;
using Library_Management_System.Models;
using Library_Management_System.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Library_Management_System.Controllers
{
	[Authorize(Policy = "StudentOnly")]
	public class StudentDashboardController : Controller
	{
		private readonly IBorrowRepository _borrowRepo;
		private readonly IHubContext<NotificationHub> _hubContext;

		public StudentDashboardController(
			IBorrowRepository borrowRepo,
			IHubContext<NotificationHub> hubContext)
		{
			_borrowRepo = borrowRepo;
			_hubContext = hubContext;
		}

		public async Task<IActionResult> Index()
		{
			try
			{
				var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				var studentName = User.Identity?.Name ?? "Student";

				if (string.IsNullOrEmpty(studentId))
					return RedirectToAction("Login", "Account");

				// Get student's borrows with null safety
				var borrows = await _borrowRepo.GetBorrowsByStudentIdAsync(studentId) ?? new List<Borrow>();

				// Calculate stats with null safety
				var pendingCount = borrows.Count(b => b?.Status == "Pending");
				var approvedCount = borrows.Count(b => b?.Status == "Approved" && b?.IsReturned == false);
				var returnedCount = borrows.Count(b => b?.IsReturned == true);

				// Recent borrows (last 5)
				var recentBorrows = borrows.Take(5).ToList();

				ViewBag.StudentName = studentName;
				ViewBag.PendingCount = pendingCount;
				ViewBag.ApprovedCount = approvedCount;
				ViewBag.ReturnedCount = returnedCount;
				ViewBag.TotalBorrows = borrows.Count;
				ViewBag.RecentBorrows = recentBorrows;

				return View();
			}
			catch (Exception ex)
			{
				// Log error
				Console.WriteLine($"Error in StudentDashboard Index: {ex.Message}");

				// Return empty dashboard
				var studentName = User.Identity?.Name ?? "Student";
				ViewBag.StudentName = studentName;
				ViewBag.PendingCount = 0;
				ViewBag.ApprovedCount = 0;
				ViewBag.ReturnedCount = 0;
				ViewBag.TotalBorrows = 0;
				ViewBag.RecentBorrows = new List<Borrow>();

				return View();
			}
		}
	}
}
