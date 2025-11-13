using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThiCK.Models
{
	public class CompareModel
	{
		[Key]
		public long Id { get; set; }
		public long ProductId { get; set; }
		public string UserId { get; set; }


		[ForeignKey("ProductId")]
		public ProductModel Product { get; set; }
	}
}
