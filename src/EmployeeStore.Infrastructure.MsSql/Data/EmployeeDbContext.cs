using EmployeeStore.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeStore.Infrastructure.MsSql.Data
{
	public class EmployeeDbContext : DbContext
	{
		public EmployeeDbContext(DbContextOptions options) : base(options)
		{
		}

		public DbSet<Employee> Employees { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Employee>()
				.HasIndex(e => e.PayrollNumber)
				.IsUnique();
		}
	}
}
