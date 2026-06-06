namespace Library_Management_System.Models.ViewModels
{
	public class BookStatistics
	{
		public int TotalBooks { get; set; }
		public int AvailableBooks { get; set; }
		public int LowStockBooks { get; set; }
		public int OutOfStockBooks { get; set; }
	}
}
