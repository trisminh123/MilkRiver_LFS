using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThiCK.Models;
using ThiCK.Models.ViewModels;
using ThiCK.Repository;

namespace ThiCK.Controllers
{
	public class BrandController : Controller
	{
		private readonly DataContext _dataContext;
		private readonly UserManager<AppUserModel> _userManager;

		public BrandController(DataContext context, UserManager<AppUserModel> userManager)
		{
			_dataContext = context;
			_userManager = userManager;

		}

		public async Task<IActionResult> Index(string Slug = "", string sort_by = "", string startprice = "", string endprice = "", int page=1)
		{
			BrandModel brand = _dataContext.Brands.Where(c => c.Slug == Slug).FirstOrDefault();
			ViewBag.Brand = brand.Name;
			if (brand == null) return RedirectToAction("Index");
			IQueryable<ProductModel> productsByBrand = _dataContext.Products.Where(b => b.BrandId == brand.Id);
			var sliders = _dataContext.Sliders.Where(s => s.Status == 1).ToList();
			ViewBag.Sliders = sliders;

			if (!string.IsNullOrEmpty(sort_by))
			{
				if (sort_by == "price_increase")
					productsByBrand = productsByBrand.OrderBy(p => p.Price);
				else if (sort_by == "price_decrease")
					productsByBrand = productsByBrand.OrderByDescending(p => p.Price);
				else if (sort_by == "price_newest")
					productsByBrand = productsByBrand.OrderByDescending(p => p.Id);
				else if (sort_by == "price_oldest")
					productsByBrand = productsByBrand.OrderBy(p => p.Id);
			}

			if (!string.IsNullOrEmpty(startprice) && !string.IsNullOrEmpty(endprice))
			{
				if (decimal.TryParse(startprice, out decimal startPriceValue) && decimal.TryParse(endprice, out decimal endPriceValue))
				{
					productsByBrand = productsByBrand.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
				}
			}

			int totalItems = await productsByBrand.CountAsync();
			const int pageSize = 9; // Số sản phẩm mỗi trang
			var paginate = new Paginate(totalItems, page, pageSize);

			var pagedProducts = await productsByBrand
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
