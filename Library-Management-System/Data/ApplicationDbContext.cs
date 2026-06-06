using Library_Management_System.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Library_Management_System.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
		public DbSet<Student> Students { get; set; }

		// Add other DbSets you might have
		public DbSet<Book> Books { get; set; }
		public DbSet<Borrow> Borrows { get; set; }


		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			// Add any custom configurations here
			builder.Entity<Student>()
				.HasIndex(s => s.AspNetUserId)
				.IsUnique();
		}
		
	}
}
