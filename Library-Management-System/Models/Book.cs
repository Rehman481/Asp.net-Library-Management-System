namespace Library_Management_System.Models
{
	public class Book
	{
		public int BookId { get; set; }
		public string BookName { get; set; }
		public string Author { get; set; }
		public string Genre { get; set; }
		public string? Description { get; set; }
		public int? PublicationYear { get; set; }
		public int Quantity { get; set; }
	}
}
