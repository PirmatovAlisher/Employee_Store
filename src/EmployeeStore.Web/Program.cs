using EmployeeStore.Core.Interfaces;
using EmployeeStore.Infrastructure.MsSql.Data;
using EmployeeStore.Infrastructure.MsSql.Repositories;
using EmployeeStore.Services.EmployeeServices;
using Microsoft.EntityFrameworkCore;

namespace EmployeeStore.Web
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddControllersWithViews();
			builder.Services.AddDbContext<EmployeeDbContext>(options =>
			options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
			builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
			builder.Services.AddScoped<ICsvParserService, CsvParserService>();
			builder.Services.AddScoped<IEmployeeService, EmployeeService>();

			var app = builder.Build();


			// Ensures Data base creation and Applies pending migrations
			using (var scope = app.Services.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();
				dbContext.Database.Migrate();
			}

			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthorization();

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Employee}/{action=Index}/{id?}");

			app.Run();
		}
	}
}
