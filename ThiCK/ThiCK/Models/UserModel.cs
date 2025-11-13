using System.ComponentModel.DataAnnotations;

namespace ThiCK.Models
{
	public class UserModel
	{
		public long Id { get; set; }
		[Required(ErrorMessage = "Bạn chưa nhập Username")]
		public string Username { get; set; }
		[Required(ErrorMessage = "Bạn chưa nhập Email"),EmailAddress]
		public string Email { get; set; }
		[DataType(DataType.Password),Required(ErrorMessage = "Bạn chưa nhập Password")]
		public string Password { get; set; }

	}
}
