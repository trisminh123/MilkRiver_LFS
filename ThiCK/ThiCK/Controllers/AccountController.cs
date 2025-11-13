using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using ThiCK.Areas.Admin.Repository;
using ThiCK.Models;
using ThiCK.Models.ViewModels;
using ThiCK.Repository;
namespace ThiCK.Controllers
{
    public class AccountController : Controller
	{
		private UserManager<AppUserModel> _userManager;
		private SignInManager<AppUserModel> _signInManager;
		private readonly ThiCK.Areas.Admin.Repository.IEmailSender _emailSender;
        private readonly DataContext _dataContext;

        public AccountController(ThiCK.Areas.Admin.Repository.IEmailSender emailSender, UserManager<AppUserModel> userManager, SignInManager<AppUserModel> signInManager, DataContext dataContext)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_emailSender = emailSender;
			_dataContext = dataContext;
		}

		public IActionResult Login(string returnUrl)
		{
			return View(new LoginViewModel { ReturnUrl = returnUrl});
		}

        public async Task<IActionResult> ForgetPass(string returnUrl)
        {
            return View();
        }

		public async Task<IActionResult> SendMailForgotPass(AppUserModel user)
		{
			var checkMail = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
			if (checkMail == null)
			{
				TempData["error"] = "Email này không tồn tại";
				return RedirectToAction("ForgetPass", "Account");
			}
			else
			{
				string token = Guid.NewGuid().ToString();
				//update token to user
				checkMail.Token = token;
				_dataContext.Update(checkMail);
				await _dataContext.SaveChangesAsync();
				var receiver = checkMail.Email;
				var subject = "Change password for user + checkMail.Email";
				var message = "Click vào đây để đổi mật khẩu: " 
				 + $"{Request.Scheme}://{Request.Host}/Account/NewPass?email=" + checkMail.Email + "&token=" + token;
				await _emailSender.SendEmailAsync(receiver, subject, message);
			}
			TempData["success"] = "Đã gửi email lấy lại mật khẩu, bạn hãy kiểm tra nhé";
			return RedirectToAction("ForgetPass", "Account");
		}
		public async Task<IActionResult> NewPass(AppUserModel user, string token)
		{
			var checkuser = await _userManager.Users
							.Where(u => u.Email == user.Email)
							.Where(u => u.Token == user.Token).FirstOrDefaultAsync();
			if (checkuser != null)
			{
				ViewBag.Email = checkuser.Email;
				ViewBag.Token = token;
			}
			else
			{
				TempData["error"] = "Không tìm thấy email hoặc token sai";
				return RedirectToAction("Forgot Pass", "Account");
			}
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> UpdateNewPassword(AppUserModel user, string token)
		{
			var checkuser = await _userManager.Users
			.Where(u => u.Email == user.Email)
			.Where(u => u.Token == user.Token).FirstOrDefaultAsync();
			if (checkuser != null)
			{
				//update user with new password and token
				string newtoken = Guid.NewGuid().ToString();
				// Hash the new password
				var passwordHasher = new PasswordHasher<AppUserModel>();
				var passwordHash = passwordHasher.HashPassword(checkuser, user.PasswordHash);
				checkuser.PasswordHash = passwordHash;
				checkuser.Token = newtoken;
				await _userManager.UpdateAsync(checkuser);
				TempData["success"] = "Đổi mật khẩu thành công.";
				return RedirectToAction("Login", "Account");
			}
			else
			{
				TempData["error"] = "Không tìm thấy email hoặc token";
				return RedirectToAction("ForgetPass", "Account");
			}
		}
		[HttpPost]
		public async Task<IActionResult> Login(LoginViewModel loginVM)
		{
			if (ModelState.IsValid)
			{
				Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager .PasswordSignInAsync(loginVM.Username, loginVM.Password,false,false);
				if (result.Succeeded)
				{
					TempData["success"] = "Xin chào " + loginVM.Username;
					return Redirect(loginVM.ReturnUrl ?? "/");
				}
				ModelState.AddModelError("", "Sai Username hoặc Password");
			}
			return View(loginVM);
		}
		public async Task<IActionResult> Logout(string returnUrl = "/")
		{
			await _signInManager.SignOutAsync();
			return Redirect(returnUrl);
		}

		
		public async Task<IActionResult> History()
		{
			if ((bool)!User.Identity?.IsAuthenticated)
			{
				// User is not logged in, redirect to login
				return RedirectToAction("Login", "Account"); // Replace "Account" with your controller name
			}
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var userEmail = User.FindFirstValue(ClaimTypes.Email);
			var Orders = await _dataContext.Orders
			.Where(od => od.UserName == userEmail).OrderByDescending(od => od.Id).ToListAsync();
			ViewBag.UserEmail = userEmail;
			return View(Orders);
		}
		public async Task<IActionResult> UpdateAccount()
		{
			if ((bool)!User.Identity?.IsAuthenticated)
			{
				// User is not logged in, redirect to login
				return RedirectToAction("Login", "Account");
			}

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return NotFound();
			}

			// Tìm người dùng bằng ID
			var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
			if (user == null)
			{
				return NotFound();
			}

			return View(user);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateInfoAccount(AppUserModel user)
		{
			// Lấy ID người dùng hiện tại
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			// Tìm người dùng theo ID
			var userById = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
			if (userById == null)
			{
				return NotFound();
			}

			// Cập nhật thông tin
			if (!string.IsNullOrEmpty(user.UserName))
			{
				userById.UserName = user.UserName; // Cập nhật tên người dùng
			}

			if (!string.IsNullOrEmpty(user.Email))
			{
				// Kiểm tra xem email có thay đổi không và có hợp lệ không
				var emailExists = await _userManager.Users.AnyAsync(u => u.Email == user.Email && u.Id != userId);
				if (emailExists)
				{
					ModelState.AddModelError("Email", "Email đã được sử dụng bởi người dùng khác.");
					TempData["error"] = "Email đã tồn tại.";
					return RedirectToAction("UpdateAccount", "Account");
				}
				userById.Email = user.Email; // Cập nhật email
			}

			if (!string.IsNullOrEmpty(user.PhoneNumber))
			{
				userById.PhoneNumber = user.PhoneNumber; // Cập nhật số điện thoại
			}

			// Lưu thay đổi thông qua UserManager
			var result = await _userManager.UpdateAsync(userById);
			if (!result.Succeeded)
			{
				// Xử lý lỗi nếu cập nhật không thành công
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
				TempData["error"] = "Cập nhật thông tin tài khoản thất bại.";
				return RedirectToAction("UpdateAccount", "Account");
			}

			// Lưu thành công
			TempData["success"] = "Cập nhật thông tin tài khoản thành công.";
			return RedirectToAction("UpdateAccount", "Account");
		}

		public async Task<IActionResult> CancelOrder(string ordercode)
		{
			if ((bool)!User.Identity?.IsAuthenticated)
			{
				// Người dùng chưa đăng nhập, chuyển hướng đến trang đăng nhập
				return RedirectToAction("Login", "Account");
			}

			try
			{
				// Lấy thông tin đơn hàng
				var order = await _dataContext.Orders
					.Where(o => o.OrderCode == ordercode)
					.FirstOrDefaultAsync();

				if (order == null)
				{
					return NotFound("Order not found.");
				}

				// Lấy danh sách chi tiết đơn hàng
				var orderDetails = await _dataContext.OrderDetails
					.Where(od => od.OrderCode == ordercode)
					.ToListAsync();

				foreach (var item in orderDetails)
				{
					// Lấy sản phẩm tương ứng
					var product = await _dataContext.Products
						.Where(p => p.Id == item.ProductId)
						.FirstOrDefaultAsync();

					if (product != null)
					{
						// Cộng lại số lượng tồn kho
						product.Quantity += item.Quantity;
						_dataContext.Update(product);
						// Thêm bản ghi mới vào ProductQuantities
						var productQuantityUpdate = new ProductQuantityModel
						{
							ProductId = item.ProductId,
							Quantity = item.Quantity, // Số lượng tăng lại
							DateCreated = DateTime.Now
						};
						_dataContext.Add(productQuantityUpdate);
					}
				}

				// Cập nhật trạng thái đơn hàng
				order.Status = 3; // Đã hủy
				_dataContext.Update(order);

				// Lưu thay đổi vào cơ sở dữ liệu
				await _dataContext.SaveChangesAsync();

				TempData["success"] = "Đã hủy đơn hàng.";
			}
			catch (Exception ex)
			{
				return BadRequest("An error occurred while canceling the order.");
			}

			return RedirectToAction("History", "Account");
		}

		public IActionResult Create()
		{
			return View();
		}
		[HttpPost]

		public async Task<IActionResult> Create(UserModel user)
		{
			if (ModelState.IsValid)
			{
				AppUserModel newUser = new AppUserModel { UserName = user.Username, Email = user.Email };
				IdentityResult result = await _userManager.CreateAsync(newUser,user.Password);
				var roleAssignResult = await _userManager.AddToRoleAsync(newUser, "Customer");

				if (result.Succeeded)
				{
					TempData["success"] = "Tạo user thành công";
					return Redirect("/account/login");
				}
				foreach(IdentityError error in  result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}
			return View(user);
		}
		public async Task<IActionResult> ViewOrder(string ordercode)
		{
			var DetailsOrder = await _dataContext.OrderDetails.Include(od => od.Product).Where(od => od.OrderCode == ordercode).ToListAsync();
			var Order = _dataContext.Orders.Where(o => o.OrderCode == ordercode).First();
			ViewBag.ShippingCost = Order.ShippingCost;
			ViewBag.Status = Order.Status;
			return View(DetailsOrder);
		}
	}
}
