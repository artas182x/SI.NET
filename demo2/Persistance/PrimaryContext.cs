using demo2.Persistance.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace demo2.Persistance
{
	public class PrimaryContext : IdentityDbContext<MyUserAccount, MyRole, string>
	{
		public PrimaryContext(DbContextOptions<PrimaryContext> options) : base(options)
		{
			// nic
		}
		
		public DbSet<MyUserAccount> MyUserAccounts { get; set; }
		public DbSet<MyRole> MyRoles { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<MyUserAccount>().ToTable("Uzytkownicy");
			modelBuilder.Entity<MyRole>().ToTable("MojeRole");
			base.OnModelCreating(modelBuilder);
		}
	}
}