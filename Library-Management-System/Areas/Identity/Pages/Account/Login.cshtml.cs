#nullable disable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Library_Management_System.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Library_Management_System.Areas.Identity.Pages.Account
{
	public class LoginModel : PageModel
	{
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ILogger<LoginModel> _logger;

		public LoginModel(
			SignInManager<ApplicationUser> signInManager,
			UserManager<ApplicationUser> userManager,
			ILogger<LoginModel> logger)
		{
			_signInManager = signInManager;
			_userManager = userManager;
			_logger = logger;
		}

		[BindProperty]
		public InputModel Input { get; set; }

		public IList<AuthenticationScheme> ExternalLogins { get; set; }
		public string ReturnUrl { get; set; }

		[TempData]
		public string ErrorMessage { get; set; }

		public class InputModel
		{
			[Required]
			[EmailAddress]
			public string Email { get; set; }

			[Required]
			[DataType(DataType.Password)]
			public string Password { get; set; }

			[Display(Name = "Remember me?")]
			public bool RememberMe { get; set; }
		}

		public async Task OnGetAsync(string returnUrl = null)
		{
			ReturnUrl = returnUrl ?? Url.Content("~/");

			if (!string.IsNullOrEmpty(ErrorMessage))
			{
				ModelState.AddModelError(string.Empty, ErrorMessage);
			}

			await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

			ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
		}

		public async Task<IActionResult> OnPostAsync(string returnUrl = null)
		{
			returnUrl ??= Url.Content("~/");

			ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

			if (ModelState.IsValid)
			{
				// This doesn't count login failures towards account lockout
				// To enable password failures to trigger account lockout, set lockoutOnFailure: true
				var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
				if (result.Succeeded)
				{
					_logger.LogInformation("User logged in.");

					// Get the logged-in user
					var user = await _userManager.FindByEmailAsync(Input.Email);

					if (user != null)
					{
						// Check user claims and redirect accordingly
						var claims = await _userManager.GetClaimsAsync(user);
						var roleClaim = claims.FirstOrDefault(c => c.Type == "Role");

						if (roleClaim != null)
						{
							if (roleClaim.Value == "Admin")
							{
								return RedirectToAction("Index", "AdminDashboard");
							}
							else if (roleClaim.Value == "Student")
							{
								return RedirectToAction("Index", "StudentDashboard");
							}
						}
					}

					// Default redirect if no role claim is found
					return LocalRedirect(returnUrl);
				}
				if (result.RequiresTwoFactor)
				{
					return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
				}
				if (result.IsLockedOut)
				{
					_logger.LogWarning("User account locked out.");
					return RedirectToPage("./Lockout");
				}
				else
				{
					ModelState.AddModelError(string.Empty, "Invalid login attempt.");
					return Page();
				}
			}

			// If we got this far, something failed, redisplay form
			return Page();
		}
	}
}
