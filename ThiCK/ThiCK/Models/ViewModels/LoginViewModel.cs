using System.ComponentModel.DataAnnotations;

namespace ThiCK.Models.ViewModels
{
	public class LoginViewModel
	{
		public long Id { get; set; }
		[Required(ErrorMessage = "Bạn chưa nhập Username")]
		public string Username { get; set; }
		[DataType(DataType.Password), Required(ErrorMessage = "Bạn chưa nhập Password")]
		public string Password { get; set; }
		public string ReturnUrl { get; set; }

	}
}
