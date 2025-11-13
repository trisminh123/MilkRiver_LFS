using Microsoft.EntityFrameworkCore;
using ThiCK.Models;

namespace ThiCK.Repository
{
	public class SeedData
	{
		public static void SeedingData(DataContext _context)
		{
			_context.Database.Migrate();
			
				_context.SaveChanges();
			
		}
	}
}
