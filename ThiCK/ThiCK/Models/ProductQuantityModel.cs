using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThiCK.Models
{
	public class ProductQuantityModel
	{
		[Key]
		public long Id { get; set; }
		public int Quantity { get; set; }

		public long ProductId { get; set; }
		public DateTime DateCreated { get; set; }
		[ForeignKey("ProductId")]

		public ProductModel Product { get; set; }
	}
}
