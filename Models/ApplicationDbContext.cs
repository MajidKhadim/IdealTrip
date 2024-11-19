using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
namespace IdealTrip.Models
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser,IdentityRole<Guid>,Guid>
	{
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }
        public DbSet<Proof> Proofs { get; set; }
		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<Proof>()
				.HasOne(p => p.User)
				.WithMany()
				.HasForeignKey(p => p.UserId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
