using EmployeeStore.Core.Interfaces;
using EmployeeStore.Core.Models;
using EmployeeStore.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Text;

namespace EmployeeStore.Tests.Controllers
{
	public class EmployeeControllerTests
	{
		private readonly Mock<IEmployeeService> _mockEmployeeService;
		private readonly Mock<ICsvParserService> _mockCsvParserService;
		private readonly EmployeeController _controller;

		public EmployeeControllerTests()
		{
			_mockEmployeeService = new Mock<IEmployeeService>();
			_mockCsvParserService = new Mock<ICsvParserService>();

			var services = new ServiceCollection();
			services.AddLogging();
			services.AddControllers();
			var serviceProvider = services.BuildServiceProvider();

			var httpContext = new DefaultHttpContext
			{
				RequestServices = serviceProvider

			};
			httpContext.Request.Headers.Add("X-Requested-With", "XMLHttpRequest"); // Key header

			var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());


			_controller = new EmployeeController(_mockCsvParserService.Object, _mockEmployeeService.Object)
			{
				ControllerContext = new ControllerContext
				{
					HttpContext = httpContext
				},
				TempData = new TempDataDictionary(
					new DefaultHttpContext(),
					Mock.Of<ITempDataProvider>()),
				Url = new UrlHelper(actionContext)
			};

			_controller.ObjectValidator = serviceProvider.GetRequiredService<IObjectModelValidator>();

		}


		[Fact]
		public async Task UploadCsv_NoFile_ReturnsBadRequestWithError()
		{
			//Arrange

			// Act
			var result = await _controller.UploadCsv(null);

			// Assert
			var badResult = Assert.IsType<BadRequestObjectResult>(result);
			var importResult = Assert.IsType<CsvImportResult>(badResult.Value);
			Assert.Contains("Please select a file", importResult.Errors);
		}

		[Fact]
		public async Task UploadCsv_InvalidFileType_ReturnsBadRequestWithError()
		{
			// Arrange
			var mockFile = CreateMockFile("test.pdf", "application/pdf");


			//Act
			var result = await _controller.UploadCsv(mockFile);


			//Assert
			var badResult = Assert.IsType<BadRequestObjectResult>(result);
			var importResult = Assert.IsType<CsvImportResult>(badResult.Value);
			Assert.Contains("Please select a CSV file", importResult.Errors);
		}

		[Fact]
		public async Task UploadCsv_ParsingErrors_ReturnsCombinedErrors()
		{
			// Arrange
			var mockFile = CreateMockFile("data.csv", "text/csv");

			var errors = new List<string>() { "Missing required field", "Invalid Email format" };

			var parseResult = new CsvImportResult()
			{
				Errors = errors
			};

			_mockCsvParserService.Setup(s => s.ParseEmployees(It.IsAny<Stream>()))
				.Returns(parseResult);

			// Act
			var result = await _controller.UploadCsv(mockFile);


			// Assert
			var badResult = Assert.IsType<BadRequestObjectResult>(result);
			var importResult = Assert.IsType<CsvImportResult>(badResult.Value);
			Assert.Equal(2, importResult.Errors.Count);
			Assert.Contains("Missing required field", importResult.Errors);

		}

		[Fact]
		public async Task UploadCsv_ValidDataWithImportErrors_ReturnsCombinedResults()
		{
			// Arrange
			var mockFile = CreateMockFile("data.csv", "text/csv");
			var parseResult = new CsvImportResult
			{
				ValidEmployees = { new Employee(), new Employee() },
				Errors = { "Warning: Duplicate entries" }
			};

			var importResult = new CsvImportResult
			{
				ValidEmployees = { parseResult.ValidEmployees.First() },
				Errors = { "Database constraint violation" }
			};

			_mockCsvParserService.Setup(s => s.ParseEmployees(It.IsAny<Stream>()))
				.Returns(parseResult);

			_mockEmployeeService.Setup(s => s.ImportEmployeesAsync(parseResult.ValidEmployees))
				.ReturnsAsync(importResult);

			// Act
			var result = await _controller.UploadCsv(mockFile);

			// Assert
			var okResult = Assert.IsType<BadRequestObjectResult>(result);
			var finalResult = Assert.IsType<CsvImportResult>(okResult.Value);

			Assert.Equal(importResult.SuccessCount, finalResult.SuccessCount);
			Assert.Equal(2, finalResult.Errors.Count);
			Assert.Contains("Warning: Duplicate entries", finalResult.Errors);
			Assert.Contains("Database constraint violation", finalResult.Errors);
		}

		[Fact]
		public async Task UploadCsv_ValidDataWithOutImportErrors_ReturnsOkResult()
		{
			// Arrange
			var mockFile = CreateMockFile("data.csv", "text/csv");
			var parseResult = new CsvImportResult
			{
				ValidEmployees = { new Employee(), new Employee() }
			};

			var importResult = new CsvImportResult
			{
				ValidEmployees = { new Employee(), new Employee() }
			};

			_mockCsvParserService.Setup(s => s.ParseEmployees(It.IsAny<Stream>()))
				.Returns(parseResult);

			_mockEmployeeService.Setup(s => s.ImportEmployeesAsync(parseResult.ValidEmployees))
				.ReturnsAsync(importResult);

			// Act
			var result = await _controller.UploadCsv(mockFile);

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			var finalResult = Assert.IsType<CsvImportResult>(okResult.Value);

			Assert.Equal(importResult.SuccessCount, finalResult.SuccessCount);
		}

		[Fact]
		public async Task UploadCsv_DatabaseException_ReturnsErrorMessage()
		{
			// Arrange
			var mockFile = CreateMockFile("data.csv", "text/csv");

			_mockCsvParserService.Setup(s => s.ParseEmployees(It.IsAny<Stream>()))
				.Throws(new Exception("CSV parsing failed"));

			// Act
			var result = await _controller.UploadCsv(mockFile);

			// Assert
			var badRequest = Assert.IsType<BadRequestObjectResult>(result);
			var importResult = Assert.IsType<CsvImportResult>(badRequest.Value);
			Assert.Contains("CSV parsing failed", importResult.Errors);
		}




		[Fact]
		public async Task Delete_ValidEmployee_ReturnsOkResult()
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

			_mockEmployeeService.Setup(s => s.DeleteEmployeeAsync(employee))
				.ReturnsAsync(new OperationResult<Employee>()
				{
					IsSuccess = true
				});


			// Act
			var result = await _controller.Delete(employee);


			// Assert
			Assert.IsType<OkResult>(result);
			_mockEmployeeService.Verify(s => s.DeleteEmployeeAsync(employee), Times.Once);
		}

		[Fact]
		public async Task Delete_EmployeeNotFound_ReturnsNotFoundWithErrors()
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
			var errorMessages = new List<string> { "Employee not found" };

			_mockEmployeeService.Setup(s => s.DeleteEmployeeAsync(employee))
				.ReturnsAsync(new OperationResult<Employee>()
				{
					IsSuccess = false,
					ErrorMessages = errorMessages
				});


			// Act
			var result = await _controller.Delete(employee);


			// Assert
			var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
			Assert.Equal(errorMessages, notFoundResult.Value);
			Assert.Equal(errorMessages, _controller.TempData["ErrorDetails"]);
			_mockEmployeeService.Verify(s => s.DeleteEmployeeAsync(employee), Times.Once);
		}

		[Fact]
		public async Task Delete_ServiceReturnsErrors_ReturnsNotFoundWithMessages()
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
			var errorMessages = new List<string> { "Database connection failed" };

			_mockEmployeeService.Setup(s => s.DeleteEmployeeAsync(employee))
				.ReturnsAsync(new OperationResult<Employee>()
				{
					IsSuccess = false,
					ErrorMessages = errorMessages
				});


			// Act
			var result = await _controller.Delete(employee);


			// Assert
			var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
			Assert.Equal(errorMessages, notFoundResult.Value);
			Assert.Equal(errorMessages, _controller.TempData["ErrorDetails"]);
		}



		[Fact]
		public async Task Update_InvalidModel_ReturnsBadRequest()
		{
			// Arrange

			// Force invalid model state
			_controller.ModelState.AddModelError("Surname", "Surname is required");
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


			// Act
			var result = await _controller.Update(employee);


			// Assert
			var badRequest = Assert.IsType<BadRequestObjectResult>(result);
			Assert.IsType<SerializableError>(badRequest.Value);
			_mockEmployeeService.Verify(s => s.UpdateEmployeeAsync(It.IsAny<Employee>()), Times.Never);
		}

		[Fact]
		public async Task Update_ValidModelServiceFails_ReturnsBadRequestWithErrors()
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
			var errorMessages = new List<string> { "Employee not found" };


			_mockEmployeeService.Setup(s => s.UpdateEmployeeAsync(employee))
				.ReturnsAsync(new OperationResult<Employee>()
				{
					IsSuccess = false,
					ErrorMessages = errorMessages
				});



			// Act
			var result = await _controller.Update(employee);

			// Assert
			var badRequest = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal(errorMessages, badRequest.Value);
			Assert.Equal(errorMessages, _controller.TempData["ErrorDetails"]);
			_mockEmployeeService.Verify(s => s.UpdateEmployeeAsync(employee), Times.Once);
		}

		[Fact]
		public async Task Update_ValidModelServiceSucceeds_ReturnsOk()
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

			_mockEmployeeService.Setup(s => s.UpdateEmployeeAsync(employee))
				.ReturnsAsync(new OperationResult<Employee>()
				{
					IsSuccess = true
				});

			// Act
			var result = await _controller.Update(employee);


			// Assert
			Assert.IsType<OkResult>(result);
			_mockEmployeeService.Verify(s => s.UpdateEmployeeAsync(employee), Times.Once);

		}

		[Fact]
		public async Task Update_ServiceThrowsException_PropagatesError()
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
			var errorMessages = "Database error";

			_mockEmployeeService.Setup(s => s.UpdateEmployeeAsync(employee))
				.ThrowsAsync(new Exception(errorMessages));


			//  Act & Assert
			await Assert.ThrowsAsync<Exception>(() => _controller.Update(employee));
		}


		private static IFormFile CreateMockFile(string fileName, string contentType)
		{
			var bytes = Encoding.UTF8.GetBytes("test data");
			var stream = new MemoryStream(bytes);

			return new FormFile(stream, 0, stream.Length, "file", fileName)
			{
				Headers = new HeaderDictionary(),
				ContentType = contentType
			};
		}
	}
}
