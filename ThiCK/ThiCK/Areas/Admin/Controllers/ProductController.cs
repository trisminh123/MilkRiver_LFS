using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ThiCK.Models;
using ThiCK.Repository;

namespace ThiCK.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/Product")]
	[Authorize(Roles = "Admin,SalesManager,Employee")]
	public class ProductController : Controller
	{
		private readonly DataContext _dataContext;
		private readonly IWebHostEnvironment _webHostEnvironment;
		public ProductController(DataContext context, IWebHostEnvironment webHostEnvironment)
		{
			_dataContext = context;
			_webHostEnvironment = webHostEnvironment;
		}
		[Route("Index")]
		public async Task<IActionResult> Index()
		{

			return View(await _dataContext.Products.OrderByDescending(p => p.Id).Include(p => p.Category).Include(p => p.Brand).ToListAsync());
		}
		[Route("Create")]
		public IActionResult Create()
		{
			ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
			ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");



			return View();
		}
		[Route("Create")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(ProductModel product)
		{
			ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
			ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

			if (ModelState.IsValid)
			{

				product.Slug = product.Name.Replace(" ", "-");
				var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
				if (slug != null)
				{
					ModelState.AddModelError("", "Sản phẩm đã có trong database");
					return View(product);
				}
				if (product.ImageUpload != null)
				{
					string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
					string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
					string filePath = Path.Combine(uploadsDir, imageName);

					FileStream fs = new FileStream(filePath, FileMode.Create);
					await product.ImageUpload.CopyToAsync(fs);
					fs.Close();
					product.Image = imageName;
				}

				_dataContext.Add(product);
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Thêm sản phẩm thành công";
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


			return View(product);
		}
		[Route("Edit")]

		public async Task<IActionResult> Edit(long Id)
		{
			ProductModel product = await _dataContext.Products.FindAsync(Id);
			ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
			ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

			return View(product);
		}

		[Route("Edit")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(ProductModel product)
		{
			ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
			ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

			var existed_product = _dataContext.Products.Find(product.Id); // tìm sản phẩm theo ID
			if (existed_product == null)
			{
				TempData["error"] = "Sản phẩm không tồn tại.";
				return RedirectToAction("Index");
			}

			if (ModelState.IsValid)
			{
				product.Slug = product.Name.Replace(" ", "-");

				if (product.ImageUpload != null)
				{
					// Đường dẫn thư mục chứa ảnh
					string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");

					// Tạo tên file mới cho ảnh
					string imageName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(product.ImageUpload.FileName);
					string newFilePath = Path.Combine(uploadsDir, imageName);

					// Đường dẫn file ảnh cũ
					string oldFilePath = Path.Combine(uploadsDir, existed_product.Image);

					try
					{
						// Xóa ảnh cũ nếu tồn tại
						if (!string.IsNullOrEmpty(existed_product.Image) && System.IO.File.Exists(oldFilePath))
						{
							System.IO.File.Delete(oldFilePath);
						}

						// Upload ảnh mới
						using (var fileStream = new FileStream(newFilePath, FileMode.Create))
						{
							await product.ImageUpload.CopyToAsync(fileStream);
						}

						// Cập nhật tên ảnh vào sản phẩm
						existed_product.Image = imageName;
					}
					catch (Exception ex)
					{
						ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật ảnh sản phẩm: " + ex.Message);
						return View(product); // Trả về view nếu có lỗi
					}
				}

				// Cập nhật các thuộc tính khác của sản phẩm
				existed_product.Name = product.Name;
				existed_product.Description = product.Description;
				existed_product.Price = product.Price;
				existed_product.CategoryId = product.CategoryId;
				existed_product.BrandId = product.BrandId;

				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Cập nhật sản phẩm thành công.";
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

			return View(product);
		}

		[Route("Delete")]
		public async Task<IActionResult> Delete(long Id)
		{
			ProductModel product = await _dataContext.Products.FindAsync(Id);

			if (product == null)
			{
				return NotFound();
			}

			string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
			string oldfilePath = Path.Combine(uploadsDir, product.Image);

			try
			{
				if (System.IO.File.Exists(oldfilePath))
				{
					System.IO.File.Delete(oldfilePath);
				}
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "An error occurred while deleting the product image.");
			}

			_dataContext.Products.Remove(product);
			await _dataContext.SaveChangesAsync();
			TempData["success"] = "Đã xóa sản phẩm thành công";
			return RedirectToAction("Index");
		}
		[Route("AddQuantity")]
		public async Task<IActionResult> AddQuantity(long Id)
		{
			var productbyQuantity = await _dataContext.ProductQuantities.Where(pq => pq.ProductId == Id).ToListAsync();
			ViewBag.ProductByQuantity = productbyQuantity;
			ViewBag.Id = Id;
			return View();
		}

		[Route("StoreProductQuantity")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult StoreProductQuantity(ProductQuantityModel productQuantityModel)
		{

			// Get the product to update
			var product = _dataContext.Products.Find(productQuantityModel.ProductId);
			if (product == null)
			{
				return NotFound(); // Handle product not found scenario
			}
			product.Quantity += productQuantityModel.Quantity;
			productQuantityModel.ProductId = productQuantityModel.ProductId;
			productQuantityModel.DateCreated = DateTime.Now;

			_dataContext.Add(productQuantityModel);
			_dataContext.SaveChangesAsync();
			TempData["success"] = "Thêm số lượng sản phẩm thành công";
			return RedirectToAction("AddQuantity", "Product", new { Id = productQuantityModel.ProductId});
		}

		[Route("DecreaseQuantity")]
		public async Task<IActionResult> DecreaseQuantity(long Id)
		{
			var productbyQuantity = await _dataContext.ProductQuantities.Where(pq => pq.ProductId == Id).ToListAsync();
			ViewBag.ProductByQuantity = productbyQuantity;
			ViewBag.Id = Id;
			return View();
		}

		[Route("DecreaseQuantity")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult DecreaseQuantity(ProductQuantityModel productQuantityModel)
		{
			// Get the product to update
			var product = _dataContext.Products.Find(productQuantityModel.ProductId);
			if (product == null)
			{
				return NotFound(); // Handle product not found scenario
			}
			if(product.Quantity > 0 &&  product.Quantity >= productQuantityModel.Quantity)
			{
				productQuantityModel.Quantity = -productQuantityModel.Quantity;
				product.Quantity += productQuantityModel.Quantity;
				productQuantityModel.ProductId = productQuantityModel.ProductId;
				productQuantityModel.DateCreated = DateTime.Now;

				_dataContext.Add(productQuantityModel);
				_dataContext.SaveChangesAsync();
				TempData["success"] = "Giảm số lượng sản phẩm thành công";
			}
			else
			{
				TempData["error"] = "Không thể giảm quá số lượng sản phẩm hiện tại";

			}
			return RedirectToAction("DecreaseQuantity", "Product", new { Id = productQuantityModel.ProductId });

		}
	}
}

