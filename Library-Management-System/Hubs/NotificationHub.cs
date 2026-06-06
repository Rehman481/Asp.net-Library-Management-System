using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Library_Management_System.Hubs
{
	[Authorize]
	public class NotificationHub : Hub
	{
		// When a user connects
		public override async Task OnConnectedAsync()
		{
			var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
			var roles = Context.User?.FindAll(ClaimTypes.Role);

			if (!string.IsNullOrEmpty(userId))
			{
				// Add to user's personal group
				await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

				// Add admins to admin group
				if (roles.Any(r => r.Value == "Admin" || r.Value == "Librarian"))
				{
					await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
				}

				// Add students to students group
				if (roles.Any(r => r.Value == "Student"))
				{
					await Groups.AddToGroupAsync(Context.ConnectionId, "students");
				}
			}

			await base.OnConnectedAsync();
		}

		// When a user disconnects
		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

			if (!string.IsNullOrEmpty(userId))
			{
				await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
				await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admins");
				await Groups.RemoveFromGroupAsync(Context.ConnectionId, "students");
			}

			await base.OnDisconnectedAsync(exception);
		}

		// Send notification to all admins
		public async Task SendToAdmins(string message)
		{
			await Clients.Group("admins").SendAsync("ReceiveNotification", message);
		}

		// Send notification to all students
		public async Task SendToStudents(string message)
		{
			await Clients.Group("students").SendAsync("ReceiveNotification", message);
		}

		// Send notification to specific user
		public async Task SendToUser(string userId, string message)
		{
			await Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", message);
		}

		// Send notification to everyone
		public async Task SendToAll(string message)
		{
			await Clients.All.SendAsync("ReceiveNotification", message);
		}

		// Admin broadcast to all users
		public async Task AdminBroadcast(string message)
		{
			var adminName = Context.User?.Identity?.Name ?? "Admin";
			await Clients.All.SendAsync("ReceiveNotification",
				$"📢 Admin Broadcast from {adminName}: {message}");
		}
	}
}