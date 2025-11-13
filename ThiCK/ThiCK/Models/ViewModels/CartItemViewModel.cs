namespace ThiCK.Models.ViewModels
{
	public class CartItemViewModel
	{
		public List<CartItemModel> CartItems { get; set;}
		public decimal GrandTotal { get; set;}
		public decimal ShippingCost { get; set; } = 10000;
	}
}
