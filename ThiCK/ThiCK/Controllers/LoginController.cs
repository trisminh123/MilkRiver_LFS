using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ThiCK.Models;

namespace ThiCK.Controllers
{
	public class LoginController : Controller
	{
		private UserManager<AppUserModel> _userManager;
		private SignInManager<AppUserModel> _signInManager;
		public LoginController(UserManager<AppUserModel> userManager, SignInManager<AppUserModel> signInManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}
		public IActionResult Index()
		{
			return View();
		}
		public async Task<IActionResult> Login()
		{
			return View();
		}
	}

}
