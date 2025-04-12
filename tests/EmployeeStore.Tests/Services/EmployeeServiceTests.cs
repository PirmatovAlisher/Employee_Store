using EmployeeStore.Core.Interfaces;
using EmployeeStore.Core.Models;
using EmployeeStore.Services.EmployeeServices;
using FluentAssertions;
using Moq;
using System.Data.Common;

namespace EmployeeStore.Tests.Services
{
	public class EmployeeServiceTests
	{
		private readonly Mock<IEmployeeRepository> _mockRepository;
		private readonly IEmployeeService _employeeService;

		public EmployeeServiceTests()
		{
			_mockRepository = new Mock<IEmployeeRepository>();
			_employeeService = new EmployeeService(_mockRepository.Object);
		}

		[Fact]
		public async Task ImportEmployeesAsync_ShouldAddSuccessfully_AllValidEmployees()
		{
			// Arrange
			var employees = new List<Employee>
			{
				new Employee {
				Id = new Guid("00000000-0000-0000-0000-000000000001"),
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
				Id = new Guid("00000000-0000-0000-0000-000000000002"),
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
				new Employee {
				Id = new Guid("00000000-0000-0000-0000-000000000003"),
				PayrollNumber = "EMP003",
				Forenames = "Michael",
				Surname = "Johnson",
				DateOfBirth = "03-03-1979",
				Telephone = "0123456791",
				Mobile = "07123456781",
				Address = "789 Park Ave",
				Address2 = "",
				Postcode = "EF5 6GH",
				EmailHome = "michael.johnson@example.com",
				StartDate = "20-05-2012"
			}
			};

			_mockRepository.Setup(r => r.GetAllEmployees())
				.Returns(new List<Employee>().AsQueryable());
			_mockRepository.Setup(r => r.AddEmployeesAsync(It.IsAny<List<Employee>>()))
				.Returns(Task.CompletedTask);

			// Act
			var result = await _employeeService.ImportEmployeesAsync(employees);


			// Assert
			Assert.Equal(employees.Count, result.SuccessCount);
			Assert.Empty(result.Errors);
			_mockRepository.Verify(r => r.AddEmployeesAsync(It.Is<List<Employee>>(list => list.Count == employees.Count)), Times.Once);
		}

		[Fact]
		public async Task ImportEmployeesAsync_ShouldReturnErrorsAndAddValid_WithDuplicates()
		{
			// Arrange
			var employees = new List<Employee>
			{
				new Employee {
				Id = new Guid("00000000-0000-0000-0000-000000000001"),
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
				Id = new Guid("00000000-0000-0000-0000-000000000002"),
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
				new Employee {
				Id = new Guid("00000000-0000-0000-0000-000000000003"),
				PayrollNumber = "EMP003",
				Forenames = "Michael",
				Surname = "Johnson",
				DateOfBirth = "03-03-1979",
				Telephone = "0123456791",
				Mobile = "07123456781",
				Address = "789 Park Ave",
				Address2 = "",
				Postcode = "EF5 6GH",
				EmailHome = "michael.johnson@example.com",
				StartDate = "20-05-2012"
			}
			};

			var existingEmployees = new List<Employee>
			{
				new Employee {
				Id = new Guid("00000000-0000-0000-0000-000000000001"),
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
				Id = new Guid("00000000-0000-0000-0000-000000000002"),
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
			}
			};

			_mockRepository.Setup(r => r.GetAllEmployees())
				.Returns(existingEmployees.AsQueryable());

			_mockRepository.Setup(r => r.AddEmployeesAsync(It.IsAny<List<Employee>>()))
				.Returns(Task.CompletedTask);

			// Act
			var result = await _employeeService.ImportEmployeesAsync(employees);

			var invalidEmployees = employees.Where(e => !result.ValidEmployees.Any(valid => valid.Id == e.Id)).ToList();


			// Assert
			Assert.NotEqual(employees.Count, result.SuccessCount);
			foreach (var employee in invalidEmployees)
			{
				Assert.Contains($"Payroll number {employee.PayrollNumber} already exists in system", result.Errors);

			}
			_mockRepository.Verify(r => r
			.AddEmployeesAsync(It.Is<List<Employee>>(list => list.Count == result.SuccessCount)), Times.Once);
		}

		[Fact]
		public async Task ImportEmployeesAsync_ShouldReturnError_WhenDbExceptionOccurs()
		{
			// Arrange
			var employees = new List<Employee>
			{
				new Employee {
				Id = new Guid("00000000-0000-0000-0000-000000000001"),
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
				Id = new Guid("00000000-0000-0000-0000-000000000002"),
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
				new Employee {
				Id = new Guid("00000000-0000-0000-0000-000000000003"),
				PayrollNumber = "EMP003",
				Forenames = "Michael",
				Surname = "Johnson",
				DateOfBirth = "03-03-1979",
				Telephone = "0123456791",
				Mobile = "07123456781",
				Address = "789 Park Ave",
				Address2 = "",
				Postcode = "EF5 6GH",
				EmailHome = "michael.johnson@example.com",
				StartDate = "20-05-2012"
			}
			};

			var mockDbException = new Mock<DbException>("Database failure", new Exception("Connection failed"));
			var dbException = mockDbException.Object;

			_mockRepository.Setup(r => r.GetAllEmployees())
				.Returns(new List<Employee>().AsQueryable());

			_mockRepository.Setup(r => r.AddEmployeesAsync(It.IsAny<List<Employee>>()))
				.ThrowsAsync(dbException);



			// Act
			var result = await _employeeService.ImportEmployeesAsync(employees);


			// Assert
			Assert.Single(result.Errors);
			Assert.Contains($"Database error: Connection failed", result.Errors);
			Assert.Equal(0, result.SuccessCount);
		}



		[Fact]
		public async Task DeleteEmployeeAsync_ShouldReturnNotFound_WhenEmployeeDoesNotExist()
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

			_mockRepository
				.Setup(r => r.GetEmployeeByIdAsync(employee.Id))
				.ReturnsAsync((Employee)null);



			// Act
			var result = await _employeeService.DeleteEmployeeAsync(employee);



			// Assert
			Assert.False(result.IsSuccess);
			Assert.Contains("Employee not found", result.ErrorMessages);
			_mockRepository.Verify(r => r.DeleteEmployeeAsync(It.IsAny<Employee>()), Times.Never);
		}

		[Fact]
		public async Task DeleteEmployeeAsync_ShouldReturnSuccess_WhenEmployeeExistsAndDeleteSucceeds()
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

			_mockRepository
				.Setup(r => r.GetEmployeeByIdAsync(employee.Id))
				.ReturnsAsync(employee);

			_mockRepository
				.Setup(r => r.DeleteEmployeeAsync(employee))
				.Returns(Task.CompletedTask);


			// Act
			var result = await _employeeService.DeleteEmployeeAsync(employee);


			// Assert
			Assert.True(result.IsSuccess);
			Assert.Empty(result.ErrorMessages);
			_mockRepository.Verify(r => r.DeleteEmployeeAsync(employee), Times.Once);
		}

		[Fact]
		public async Task DeleteEmployeeAsync_ShouldReturnError_WhenExceptionOccurs()
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
			var exceptionMessage = "Database error";

			_mockRepository
				.Setup(r => r.GetEmployeeByIdAsync(employee.Id))
				.ReturnsAsync(employee);

			_mockRepository
				.Setup(r => r.DeleteEmployeeAsync(employee))
				.ThrowsAsync(new Exception(exceptionMessage));



			// Act
			var result = await _employeeService.DeleteEmployeeAsync(employee);



			// Assert
			Assert.False(result.IsSuccess);
			Assert.Contains($"An error occurred while deleting the employee with Payroll number \"{employee.PayrollNumber}\" : {exceptionMessage}",
				result.ErrorMessages);
		}




		[Fact]
		public async Task UpdateEmployeeAsync_ShouldReturnNotFound_WhenEmployeeDoesNotExist()
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

			_mockRepository
				.Setup(r => r.UpdateEmployeeAsync(employee))
				.ReturnsAsync(false);



			// Act
			var result = await _employeeService.UpdateEmployeeAsync(employee);



			// Assert
			Assert.False(result.IsSuccess);
			Assert.Contains("Employee not found", result.ErrorMessages);
			_mockRepository.Verify(r => r.UpdateEmployeeAsync(employee), Times.Once);
		}

		[Fact]
		public async Task UpdateEmployeeAsync_ShouldReturnError_WhenExceptionOccurs()
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
			var exceptionMessage = "Database error";

			_mockRepository
				.Setup(r => r.UpdateEmployeeAsync(employee))
				.ThrowsAsync(new Exception(exceptionMessage));



			// Act
			var result = await _employeeService.UpdateEmployeeAsync(employee);



			// Assert
			Assert.False(result.IsSuccess);
			Assert.Contains($"An error occurred while updating the employee with Payroll number \"{employee.PayrollNumber}\" : {exceptionMessage}",
			result.ErrorMessages);
			_mockRepository.Verify(r => r.UpdateEmployeeAsync(employee), Times.Once);
		}

		[Fact]
		public async Task UpdateEmployeeAsync_ShouldReturnSuccess_WhenEmployeeExistsAndUpdateSucceeds()
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

			_mockRepository
				.Setup(r => r.GetEmployeeByIdAsync(employee.Id))
				.ReturnsAsync(employee);

			_mockRepository
				.Setup(r => r.UpdateEmployeeAsync(employee))
				.ReturnsAsync(true);



			// Act
			var result = await _employeeService.UpdateEmployeeAsync(employee);



			// Assert
			Assert.True(result.IsSuccess);
			Assert.Empty(result.ErrorMessages);
			_mockRepository.Verify(r => r.UpdateEmployeeAsync(employee), Times.Once);
		}
	}
}
