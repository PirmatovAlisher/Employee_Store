using EmployeeStore.Core.Models;
using EmployeeStore.Infrastructure.MsSql.Data;
using EmployeeStore.Infrastructure.MsSql.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EmployeeStore.Tests.Repositories
{
	public class EmployeeRepositoryTests : IDisposable
	{
		private readonly EmployeeDbContext _context;
		private readonly EmployeeRepository _repository;

		public EmployeeRepositoryTests()
		{
			var options = new DbContextOptionsBuilder<EmployeeDbContext>()
				.UseInMemoryDatabase(databaseName: "TestDb")
				.Options;

			_context = new EmployeeDbContext(options);
			_context.Database.EnsureCreated();
			_repository = new EmployeeRepository(_context);
		}

		[Fact]
		public async Task AddEmployeesAsync_Test()
		{
			// Arrange
			var employees = new List<Employee>
			{
				new Employee
				{
				PayrollNumber = "EMP001",
				Forenames = "John",
				Surname = "Doe",
				DateOfBirth = "15-06-1980",
				Telephone = "0123456789",
				Mobile = "07123456789",
				Address = "123 Main St",
				Address2 = "",
				Postcode = "AB1 2CD",
				EmailHome = "john.doe@example.com",
				StartDate = "01-01-2010"
				},
				new Employee {
				PayrollNumber = "EMP002",
				Forenames = "Jane",
				Surname = "Smith",
				DateOfBirth = "22-08-1985",
				Telephone = "0123456790",
				Mobile = "07123456780",
				Address = "456 High St",
				Address2 = "Apt 2",
				Postcode = "CD3 4EF",
				EmailHome = "jane.smith@example.com",
				StartDate = "15-03-2011"
			},
			};

			await _repository.AddEmployeesAsync(employees);
			await _context.SaveChangesAsync();

			_context.Employees.Should().HaveCount(employees.Count);

			_context.Employees.Should().BeEquivalentTo(employees,
				options => options.Excluding(x => x.Id) // Excludes database-generated IDs
			.ComparingByMembers<Employee>());
		}

		[Fact]
		public async Task UpdateEmployeeAsync_ShouldReturnTrue_WhenEmployeeExists()
		{
			// Arrange

			var Id = Guid.NewGuid();

			var existingEmployee = new Employee
			{
				Id = Id,
				PayrollNumber = "EMP001",
				Forenames = "John",
				Surname = "Doe",
				DateOfBirth = "15-06-1980",
				Telephone = "0123456789",
				Mobile = "07123456789",
				Address = "123 Main St",
				Address2 = "",
				Postcode = "AB1 2CD",
				EmailHome = "john.doe@example.com",
				StartDate = "01-01-2010"
			};

			await _context.Employees.AddAsync(existingEmployee);
			await _context.SaveChangesAsync();

			var updatedEmployee = new Employee
			{
				Id = Id,
				PayrollNumber = "EMP002",
				Forenames = "Jane",
				Surname = "Smith",
				DateOfBirth = "22-08-1985",
				Telephone = "0123456790",
				Mobile = "07123456780",
				Address = "456 High St",
				Address2 = "Apt 2",
				Postcode = "CD3 4EF",
				EmailHome = "jane.smith@example.com",
				StartDate = "15-03-2011"
			};



			// Act
			var result = await _repository.UpdateEmployeeAsync(updatedEmployee);



			// Assert
			Assert.True(result);

			Assert.Equal(updatedEmployee.PayrollNumber, existingEmployee.PayrollNumber);
			Assert.Equal(updatedEmployee.Forenames, existingEmployee.Forenames);
			Assert.Equal(updatedEmployee.Surname, existingEmployee.Surname);
			Assert.Equal(updatedEmployee.DateOfBirth, existingEmployee.DateOfBirth);
			Assert.Equal(updatedEmployee.Mobile, existingEmployee.Mobile);
			Assert.Equal(updatedEmployee.Telephone, existingEmployee.Telephone);
			Assert.Equal(updatedEmployee.Address, existingEmployee.Address);
			Assert.Equal(updatedEmployee.Address2, existingEmployee.Address2);
			Assert.Equal(updatedEmployee.Postcode, existingEmployee.Postcode);
			Assert.Equal(updatedEmployee.EmailHome, existingEmployee.EmailHome);
			Assert.Equal(updatedEmployee.StartDate, existingEmployee.StartDate);

		}

		[Fact]
		public async Task UpdateEmployeeAsync_ShouldReturnFalse_WhenEmployeeDoesNotExist()
		{
			// Arrange
			var Id = Guid.NewGuid();
			var nonExistingEmployee = new Employee
			{
				Id = Id,
				PayrollNumber = "EMP001",
				Forenames = "John",
				Surname = "Doe",
				DateOfBirth = "15-06-1980",
				Telephone = "0123456789",
				Mobile = "07123456789",
				Address = "123 Main St",
				Address2 = "",
				Postcode = "AB1 2CD",
				EmailHome = "john.doe@example.com",
				StartDate = "01-01-2010"
			};



			// Act
			var result = await _repository.UpdateEmployeeAsync(nonExistingEmployee);



			// Assert
			Assert.False(result);
			Assert.Empty(_context.Employees);
		}

		[Fact]
		public async Task DeleteEmployeeAsync_Test()
		{
			// Arrange
			var employee = new Employee
			{
				Id = Guid.NewGuid(),
				PayrollNumber = "EMP001",
				Forenames = "John",
				Surname = "Doe",
				DateOfBirth = "15-06-1980",
				Telephone = "0123456789",
				Mobile = "07123456789",
				Address = "123 Main St",
				Address2 = "",
				Postcode = "AB1 2CD",
				EmailHome = "john.doe@example.com",
				StartDate = "01-01-2010"
			};

			await _context.Employees.AddAsync(employee);
			await _context.SaveChangesAsync();

			await _repository.DeleteEmployeeAsync(employee);


			// Act
			var deletedEmployee = await _context.Employees.FindAsync(employee.Id);


			// Assert
			Assert.Null(deletedEmployee);
			Assert.Empty(_context.Employees);
		}


		public void Dispose()
		{
			_context.Database.EnsureDeleted();
			_context.Dispose();
		}
	}
}
