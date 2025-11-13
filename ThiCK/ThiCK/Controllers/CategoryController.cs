using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThiCK.Models;
using ThiCK.Models.ViewModels;
using ThiCK.Repository;

namespace ThiCK.Controllers
{
	public class CategoryController : Controller
	{
		private readonly DataContext _dataContext;
		private readonly UserManager<AppUserModel> _userManager;

		public CategoryController(DataContext context, UserManager<AppUserModel> userManager)
		{
			_dataContext = context;
			_userManager = userManager;
		}
		public async Task<IActionResult> Index(string Slug = "", string sort_by = "", string startprice="", string endprice="", int page = 1)
		{
			CategoryModel category = _dataContext.Categories.Where(c => c.Slug == Slug).FirstOrDefault();
			ViewBag.Category = category.Name;
			if (category == null) return RedirectToAction("Index");
			IQueryable<ProductModel> productsByCategory = _dataContext.Products.Where(c => c.CategoryId == category.Id);
			var sliders = _dataContext.Sliders.Where(s => s.Status == 1).ToList();
			ViewBag.Sliders = sliders;

			if (!string.IsNullOrEmpty(sort_by))
			{
				if (sort_by == "price_increase")
					productsByCategory = productsByCategory.OrderBy(p => p.Price);
				else if (sort_by == "price_decrease")
					productsByCategory = productsByCategory.OrderByDescending(p => p.Price);
				else if (sort_by == "price_newest")
					productsByCategory = productsByCategory.OrderByDescending(p => p.Id);
				else if (sort_by == "price_oldest")
					productsByCategory = productsByCategory.OrderBy(p => p.Id);
			}

			if (!string.IsNullOrEmpty(startprice) && !string.IsNullOrEmpty(endprice))
			{
				if (decimal.TryParse(startprice, out decimal startPriceValue) && decimal.TryParse(endprice, out decimal endPriceValue))
				{
					productsByCategory = productsByCategory.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
				}
			}

			int totalItems = await productsByCategory.CountAsync();
			const int pageSize = 9; // Số sản phẩm mỗi trang
			var paginate = new Paginate(totalItems, page, pageSize);

			var pagedProducts = await productsByCategory
				.Skip((paginate.CurrentPage - 1) * pageSize)
				.Take(pageSize)
			.ToListAsync();

			ViewBag.Paginate = paginate;

			var user = await _userManager.GetUserAsync(User);
			// Lấy danh sách sản phẩm yêu thích của người dùng
			var wishlistProductIds = user != null
				? await _dataContext.Wishlists
					.Where(w => w.UserId == user.Id)
					.Select(w => w.ProductId)
					.ToListAsync()
				: new List<long>();

			// Lấy danh sách sản phẩm yêu thích của người dùng
			var compareProductIds = user != null
				? await _dataContext.Compares
					.Where(w => w.UserId == user.Id)
					.Select(w => w.ProductId)
					.ToListAsync()
				: new List<long>();

			// Kết hợp thông tin sản phẩm với trạng thái yêu thích
			var viewModel = pagedProducts.Select(product => new ProductWishlistCompareViewModel
			{
				Product = product,
				IsInWishlist = wishlistProductIds.Contains(product.Id),
				IsInCompare = compareProductIds.Contains(product.Id)
			}).ToList();

			return View(viewModel);
		}
	}
}
