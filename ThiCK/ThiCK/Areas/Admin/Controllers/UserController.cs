using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ThiCK.Models;
using ThiCK.Repository;

namespace ThiCK.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/User")]
	[Authorize(Roles = "Admin")]
	public class UserController : Controller
	{
		private readonly UserManager<AppUserModel> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly DataContext _dataContext;
		public UserController(DataContext context,UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_dataContext = context;
		}
        [Route("Index/{pg:int?}")]
        public async Task<IActionResult> Index(int pg = 1)
        {
            // Lấy danh sách người dùng cùng vai trò
            var usersWithRoles = await (from u in _dataContext.Users
                                        join ur in _dataContext.UserRoles on u.Id equals ur.UserId
                                        join r in _dataContext.Roles on ur.RoleId equals r.Id
                                        select new UserWithRoleViewModel
                                        {
                                            Id = u.Id,
                                            UserName = u.UserName,
                                            Email = u.Email,
                                            PhoneNumber = u.PhoneNumber,
                                            RoleName = r.Name
                                        }).ToListAsync();

            // Số lượng mục trên mỗi trang
            const int pageSize = 10;

            // Đảm bảo trang không nhỏ hơn 1
            if (pg < 1)
            {
                pg = 1;
            }

            // Tổng số bản ghi
            int recsCount = usersWithRoles.Count;

            // Tạo đối tượng phân trang
            var pager = new Paginate(recsCount, pg, pageSize);

            // Số bản ghi cần bỏ qua
            int recSkip = (pg - 1) * pageSize;

            // Áp dụng phân trang
            var data = usersWithRoles.Skip(recSkip).Take(pager.PageSize).ToList();

            // Gửi thông tin phân trang sang view
            ViewBag.Pager = pager;

            return View(data);
        }


        [Route("Create")]
		public async Task<IActionResult> Create()
		{
			var roles = await _roleManager.Roles.ToListAsync();
			ViewBag.Roles = new SelectList(roles, "Id", "Name");
			return View(new AppUserModel());

		}
		[Route("Create")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(AppUserModel user)
		{
			if (ModelState.IsValid)
			{
				var createUserResult = await _userManager.CreateAsync(user,user.PasswordHash);
				if (createUserResult.Succeeded)
				{
					var createUser = await _userManager.FindByEmailAsync(user.Email);// tim user dua vao email
					var userId = createUser.Id;
					var role = _roleManager.FindByIdAsync(user.RoleId);
					//gán quyền
					var addToRoleResult = await _userManager.AddToRoleAsync(createUser, role.Result.Name);
					if (!addToRoleResult.Succeeded)
					{
						AddIdentityErrors(createUserResult);

					}
					TempData["success"] = "Thêm user mới thành công";
					return RedirectToAction("Index", "User");
				}
				else
				{

					AddIdentityErrors(createUserResult);
					return View(user);
				}
			}
			else
			{
				TempData["error"] = "Model có một vài thứ đang bị lỗi";
				List<string> errors = new List<string>();
				foreach (var value in ModelState.Values)
				{
					foreach (var error in value.Errors)
					{
						errors.Add(error.ErrorMessage);
					}
				}
				string errorMessage = string.Join("\n", errors);
				return BadRequest(errorMessage);
			}
			var roles = await _roleManager.Roles.ToListAsync();
			ViewBag.Roles = new SelectList(roles, "Id", "Name");
			return View(user);
		}
		private void AddIdentityErrors(IdentityResult identityResult)
		{
			foreach(var error in identityResult.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
		}
		[Route("Edit")]
		public async Task<IActionResult> Edit(string Id)
		{
			if (string.IsNullOrEmpty(Id))
			{
				return NotFound();
			}
			var user = await _userManager.FindByIdAsync(Id);
			if (user == null)
			{
				return NotFound();
			}

			var roles = await _roleManager.Roles.ToListAsync();
			ViewBag.Roles = new SelectList(roles, "Id", "Name");
			return View(user);

		}
		[Route("Edit")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string Id, AppUserModel user)
		{
			var existingUser = await _userManager.FindByIdAsync(Id);
			if (existingUser == null)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				existingUser.UserName = user.UserName;
				existingUser.Email = user.Email;
				existingUser.PhoneNumber = user.PhoneNumber;
				existingUser.RoleId = user.RoleId;

				var updateUserResult = await _userManager.UpdateAsync(existingUser);
				if (updateUserResult.Succeeded)
				{
					TempData["success"] = "Chỉnh sửa thông tin người dùng thành công.";
					return RedirectToAction("Index", "User");
				}
				else
				{
					AddIdentityErrors(updateUserResult);
					return View(existingUser);
				}
			}

			var roles = await _roleManager.Roles.ToListAsync();
			ViewBag.Roles = new SelectList(roles, "Id", "Name");

			TempData["error"] = "Model Validation failed.";
			var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
			string errorMessage = string.Join("\n", errors);
			return View(existingUser);
		}
        [Route("Delete")]
        public async Task<IActionResult> Delete(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return NotFound();
            }

            // Lấy thông tin người dùng
            var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == Id);
            if (user == null)
            {
                return NotFound();
            }

            // Xóa các bản ghi liên quan trong bảng AspNetUserRoles
            var userRoles = _dataContext.UserRoles.Where(ur => ur.UserId == Id);
            _dataContext.UserRoles.RemoveRange(userRoles);

            // Xóa người dùng trong bảng AspNetUsers
            _dataContext.Users.Remove(user);

            // Lưu thay đổi
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Đã xóa người dùng thành công.";
            return RedirectToAction("Index");
        }

    }
}
