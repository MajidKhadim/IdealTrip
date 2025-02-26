using IdealTrip.Models.Database_Tables;
using IdealTrip.Models.LocalHome_Booking;
using IdealTrip.Models.Package_Booking;
using IdealTrip.Models.TourGuide_Booking;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Reflection.Emit;
namespace IdealTrip.Models
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser,IdentityRole<Guid>,Guid>
	{
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }
        public DbSet<Proof> Proofs { get; set; }
		public DbSet<Package> Packages { get; set; }
		public DbSet<UsersPackageBooking> UsersPackages { get; set; }
		public DbSet<TourGuide> TourGuides { get ; set; }
		public DbSet<Notifications> Notifications { get; set; }

		public DbSet<UserTourGuideBooking> UserTourGuideBookings { get; set; }
		public DbSet<Feedback> FeedBacks { get; set; }
		public DbSet<LocalHome> LocalHomes { get; set; }
		public DbSet<ServiceImage> ServiceImages { get; set; }
		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<Proof>()
				.HasOne(p => p.User)
				.WithMany()
				.HasForeignKey(p => p.UserId)
				.OnDelete(DeleteBehavior.Cascade);
			builder.Entity<ApplicationUser>()
				.HasIndex(u => u.Email)
				.IsUnique();
			builder.Entity<ApplicationUser>()
				.HasIndex(u => u.UserName)
				.IsUnique(false);
			builder.Entity<ApplicationUser>()
				.HasIndex(u => u.FullName)
				.IsUnique(false);
			builder.Entity<ApplicationUser>()
				.HasIndex(u => u.NormalizedUserName)
				.IsUnique(false);
			// Ensure Email is unique
			builder.Entity<ApplicationUser>()
				.HasIndex(u => u.Email)
				.IsUnique(true);

			builder.Entity<UserTourGuideBooking>()
		.HasOne(b => b.User)
		.WithMany()
		.HasForeignKey(b => b.UserId)
		.OnDelete(DeleteBehavior.Restrict); // Prevents cascade delete

			builder.Entity<UserTourGuideBooking>()
				.HasOne(b => b.TourGuide)
				.WithMany()
				.HasForeignKey(b => b.TourGuideId)
				.OnDelete(DeleteBehavior.Restrict); // Prevents cascade delete
		}
	}
}
