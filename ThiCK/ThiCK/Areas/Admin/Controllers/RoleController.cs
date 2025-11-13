using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThiCK.Models;
using ThiCK.Repository;

namespace ThiCK.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/Role")]
	[Authorize(Roles = "Admin")]
	public class RoleController : Controller
	{
		private readonly DataContext _dataContext;
		private readonly RoleManager<IdentityRole> _roleManager;

		public RoleController(DataContext context, RoleManager<IdentityRole> roleManager)
		{
			_dataContext = context;
			_roleManager = roleManager;
		}
		[Route("Index")]
		public async Task<IActionResult> Index()
		{

			return View(await _dataContext.Roles.OrderByDescending(p => p.Id).ToListAsync());
		}
		[Route("Create")]
		public IActionResult Create()
		{
			return View();
		}
		[Route("Create")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(IdentityRole model)
		{
			if (!await _roleManager.RoleExistsAsync(model.Name))
			{
				var role = new IdentityRole
				{
					Name = model.Name,
					ConcurrencyStamp = Guid.NewGuid().ToString()
				};

				var result = await _roleManager.CreateAsync(role);

				if (result.Succeeded)
				{
					TempData["success"] = "Đã tạo Role mới thành công";
				}
			}
			return RedirectToAction("Index");
		}


		[Route("Edit")]
		public async Task<IActionResult> Edit(string Id)
		{
			if (string.IsNullOrEmpty(Id))
			{
				return NotFound();
			}
			
			var role = await _roleManager.FindByIdAsync(Id);
			return View(role);
		}
		[HttpPost]
		[Route("Edit")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string id, IdentityRole model)
		{

			if (string.IsNullOrEmpty(id))
			{
				return NotFound();
			}
			if(ModelState.IsValid)
			{
				var role = await _roleManager.FindByIdAsync(id);
				if (role == null)
				{
					return NotFound();
				}
				role.Name = model.Name;
				try
				{
					await _roleManager.UpdateAsync(role);
					TempData["success"] = "Đã cập nhật Role thành công";
					return RedirectToAction("Index");
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", "Có lỗi khi đang cập nhật Role này");
				}
				return Redirect("Index");
			}
			return View(model ?? new IdentityRole { Id = id});
		}

		[Route("Delete")]
		public async Task<IActionResult> Delete(string Id)
		{
			if (string.IsNullOrEmpty(Id))
			{
				return NotFound();
			}

			var role = await _roleManager.FindByIdAsync(Id);

			if (role == null)
			{
				return NotFound();
			}

			try
			{
				await _roleManager.DeleteAsync(role);
				TempData["success"] = "Đã xóa Role thành công";
			}
			catch(Exception ex)
			{
				ModelState.AddModelError("", "Có lỗi khi đang thực hiện xóa Role này");
			}
			return Redirect("Index");
		}
	}
}
