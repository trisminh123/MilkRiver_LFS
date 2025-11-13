using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThiCK.Models;
using ThiCK.Repository;

namespace ThiCK.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/Brand")]
	[Authorize(Roles = "Admin,SalesManager")]
	public class BrandController : Controller
	{
		private readonly DataContext _dataContext;
		public BrandController(DataContext context)
		{
			_dataContext = context;
		}
		[Route("Index")]
		public async Task<IActionResult> Index(int pg = 1)
		{
			List<BrandModel> brand = _dataContext.Brands.ToList(); //33 datas


			const int pageSize = 10; //10 items/trang

			if (pg < 1) //page < 1;
			{
				pg = 1; //page ==1
			}
			int recsCount = brand.Count(); //33 items;

			var pager = new Paginate(recsCount, pg, pageSize);

			int recSkip = (pg - 1) * pageSize; //(3 - 1) * 10; 

			//category.Skip(20).Take(10).ToList()

			var data = brand.Skip(recSkip).Take(pager.PageSize).ToList();

			ViewBag.Pager = pager;

			return View(data);
		}
		[Route("Create")]
		public IActionResult Create()
		{
			return View();
		}
		[Route("Create")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(BrandModel brand)
		{

			if (ModelState.IsValid)
			{

				brand.Slug = brand.Name.Replace(" ", "-");
				var slug = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Slug == brand.Slug);
				if (slug != null)
				{
					ModelState.AddModelError("", "Thương hiệu đã có trong database");
					return View(brand);
				}

				_dataContext.Add(brand);
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Thêm thương hiệu thành công";
				return RedirectToAction("Index");
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


			return View(brand);
		}
		[Route("Edit")]
		public async Task<IActionResult> Edit(long Id)
		{
			BrandModel brand = await _dataContext.Brands.FindAsync(Id);
			return View(brand);
		}
		[Route("Edit")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(BrandModel brand)
		{
			var existed_brand = _dataContext.Brands.Find(brand.Id); // tìm danh mục theo ID
			if (existed_brand == null)
			{
				TempData["error"] = "Thương hiệu không tồn tại.";
				return RedirectToAction("Index");
			}
			if (ModelState.IsValid)
			{

				brand.Slug = brand.Name.Replace(" ", "-");
				var slug = await _dataContext.Brands.FirstOrDefaultAsync(p => p.Slug == brand.Slug && p.Id != brand.Id);
				if (slug != null)
				{
					ModelState.AddModelError("", "Thương hiệu đã có trong database");
					return View(brand);
				}

				existed_brand.Name = brand.Name;
				existed_brand.Description = brand.Description;
				existed_brand.Status = brand.Status;
				existed_brand.Slug = brand.Slug;
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Cập nhật thương hiệu thành công";
				return RedirectToAction("Index");
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


			return View(brand);
		}
		[Route("Delete")]
		public async Task<IActionResult> Delete(long Id)
		{
			BrandModel brand = await _dataContext.Brands.FindAsync(Id);

			_dataContext.Brands.Remove(brand);
			await _dataContext.SaveChangesAsync();
			TempData["success"] = "Đã xóa thương hiệu thành công";
			return RedirectToAction("Index");
		}
	}
}
