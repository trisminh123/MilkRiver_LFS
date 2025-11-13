using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThiCK.Models;
using ThiCK.Models.ViewModels;
using ThiCK.Repository;

namespace ThiCK.Controllers
{
	public class ProductController : Controller
	{
		private readonly DataContext _dataContext;
		private readonly UserManager<AppUserModel> _userManager;

		public ProductController(DataContext context, UserManager<AppUserModel> userManager)
		{
			_dataContext = context;
			_userManager = userManager;
		}
		public IActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> Details(long Id)
		{
			if(Id == null) return RedirectToAction("Index");
			var productsById = _dataContext.Products
				.Include(p => p.Ratings)
				.Where(c => c.Id == Id).FirstOrDefault();
			//related products
			var relatedProducts = await _dataContext.Products
				.Where(p => p.CategoryId == productsById.CategoryId && p.Id != productsById.Id)
				.Take(3)
				.ToListAsync();
			ViewBag.RelatedProducts = relatedProducts;

			var viewModel = new ProductDetailsViewModel
			{
				ProductDetails = productsById,
			};
			return View(viewModel);
		}
		public async Task<IActionResult> Search(string searchTerm, string sort_by = "", string startprice = "", string endprice = "", int page = 1)
		{
			var sliders = _dataContext.Sliders.Where(s => s.Status == 1).ToList();
			ViewBag.Sliders = sliders;

			if (string.IsNullOrEmpty(searchTerm))
			{
				ModelState.AddModelError("", "Search term cannot be empty.");
				return View(new List<ProductModel>()); // Trả về danh sách rỗng nếu từ khóa tìm kiếm rỗng
			}

			// Bắt đầu tìm kiếm
			IQueryable<ProductModel> query = _dataContext.Products
				.Include(p => p.Brand)
				.Include(p => p.Category)
				.Where(p => p.Name.Contains(searchTerm) ||
							p.Brand.Name.Contains(searchTerm));

			ViewBag.Keyword = searchTerm;

			// Lọc theo giá nếu có
			if (!string.IsNullOrEmpty(startprice) && !string.IsNullOrEmpty(endprice))
			{
				if (decimal.TryParse(startprice, out decimal startPriceValue) && decimal.TryParse(endprice, out decimal endPriceValue))
				{
					query = query.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
				}
			}

			// Sắp xếp sản phẩm
			if (!string.IsNullOrEmpty(sort_by))
			{
				if (sort_by == "price_increase")
					query = query.OrderBy(p => p.Price);
				else if (sort_by == "price_decrease")
					query = query.OrderByDescending(p => p.Price);
				else if (sort_by == "price_newest")
					query = query.OrderByDescending(p => p.Id);
				else if (sort_by == "price_oldest")
					query = query.OrderBy(p => p.Id);
			}

			// Phân trang
			int totalItems = await query.CountAsync();
			const int pageSize = 9;
			var paginate = new Paginate(totalItems, page, pageSize);

			var pagedProducts = await query
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


		public async Task<IActionResult> CommentProduct(RatingModel rating)
		{
			if(ModelState.IsValid)
			{
				var ratingEntity = new RatingModel
				{
					ProductId = rating.ProductId,
					Name = rating.Name,
					Email = rating.Email,
					Comment = rating.Comment,
					Star = rating.Star
				};

				_dataContext.Add(ratingEntity);
				await _dataContext.SaveChangesAsync();

				TempData["success"] = "Thêm đánh giá thành công";
				return Redirect(Request.Headers["Referer"]);
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
				return RedirectToAction("Details", new { id = rating.Id });
			}
			return Redirect(Request.Headers["Referer"]);
		}
		public async Task<IActionResult> ProductDetails(long productId)
		{
			var product = await _dataContext.Products
				.Include(p => p.Ratings) // Nạp kèm danh sách Ratings
				.FirstOrDefaultAsync(p => p.Id == productId);

			if (product == null)
			{
				return NotFound();
			}

			var model = new ProductDetailsViewModel
			{
				ProductDetails = product
			};

			return View(model);
		}

	}
}
