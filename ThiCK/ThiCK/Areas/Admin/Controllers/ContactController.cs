using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ThiCK.Models;
using ThiCK.Repository;

namespace ThiCK.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/Contact")]
	[Authorize(Roles = "Admin,SalesManager")]
	public class ContactController : Controller
	{
		private readonly DataContext _dataContext;
		private readonly IWebHostEnvironment _webHostEnvironment;
		public ContactController(DataContext context, IWebHostEnvironment webHostEnvironment)
		{
			_dataContext = context;
			_webHostEnvironment = webHostEnvironment;
		}
		[Route("Index")]
		public IActionResult Index()
		{
			var contact = _dataContext.Contacts.ToList();
			return View(contact);
		}
		[Route("Edit")]
		public async Task<IActionResult> Edit()
		{
			ContactModel contact = await _dataContext.Contacts.FirstOrDefaultAsync();
			return View(contact);
		}
		[Route("Edit")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(ContactModel contact)
		{
			var existed_contact = _dataContext.Contacts.FirstOrDefault(); 
			if (ModelState.IsValid)
			{
				if (contact.ImageUpload != null)
				{
					// Đường dẫn thư mục chứa ảnh
					string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/logo");

					// Tạo tên file mới cho ảnh
					string imageName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(contact.ImageUpload.FileName);
					string newFilePath = Path.Combine(uploadsDir, imageName);

					// Đường dẫn file ảnh cũ
					string oldFilePath = Path.Combine(uploadsDir, existed_contact.LogoImg);

					try
					{
						// Xóa ảnh cũ nếu tồn tại
						if (!string.IsNullOrEmpty(existed_contact.LogoImg) && System.IO.File.Exists(oldFilePath))
						{
							System.IO.File.Delete(oldFilePath);
						}

						// Upload ảnh mới
						using (var fileStream = new FileStream(newFilePath, FileMode.Create))
						{
							await contact.ImageUpload.CopyToAsync(fileStream);
						}

						// Cập nhật tên ảnh vào sản phẩm
						existed_contact.LogoImg = imageName;
					}
					catch (Exception ex)
					{
						ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật ảnh logo: " + ex.Message);
						return View(contact); // Trả về view nếu có lỗi
					}
				}

				// Cập nhật các thuộc tính khác của sản phẩm
				existed_contact.Name = contact.Name;
				existed_contact.Description = contact.Description;
				existed_contact.Email = contact.Email;
				existed_contact.Phone = contact.Phone;
				existed_contact.Map = contact.Map;

				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Cập nhật thông tin Contact thành công.";
				return RedirectToAction("Index");
			}
			else
			{
				TempData["error"] = "Model có một vài lỗi.";
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

			return View(contact);
		}
	}
}
