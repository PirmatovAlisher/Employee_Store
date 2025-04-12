using EmployeeStore.Core.Interfaces;
using EmployeeStore.Core.Models;
using EmployeeStore.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeStore.Web.Controllers
{
	public class EmployeeController : Controller
	{
		private readonly ICsvParserService _csvParser;
		private readonly IEmployeeService _employeeService;


		public EmployeeController(ICsvParserService csvParser, IEmployeeService employeeService)
		{
			_csvParser = csvParser;
			_employeeService = employeeService;
		}


		[HttpGet]
		public IActionResult Index()
		{
			var employees = _employeeService.GetAllEmployees();

			return View(employees);
		}

		[HttpPost]
		public async Task<IActionResult> UploadCsv(IFormFile file)
		{
			var result = new CsvImportResult();

			try
			{
				if (file == null || file.Length == 0)
				{
					result.Errors.Add("Please select a file");
					return HandleUploadResult(result);
				}

				if (file?.ContentType != "text/csv")
				{
					result.Errors.Add("Please select a CSV file");
					return HandleUploadResult(result);
				}

				using var stream = file.OpenReadStream();

				// Parse CSV
				var parseResult = _csvParser.ParseEmployees(stream);


				// Import data
				if (parseResult.ValidEmployees.Any())
				{
					var importResult = await _employeeService.ImportEmployeesAsync(parseResult.ValidEmployees);

					result.ValidEmployees.AddRange(importResult.ValidEmployees);
					result.Errors.AddRange(importResult.Errors);
				}

				result.Errors.AddRange(parseResult.Errors);

				return HandleUploadResult(result);
			}
			catch (Exception ex)
			{
				result.Errors.Add(ex.Message);
				return HandleUploadResult(result);
			}
		}

		[HttpPost]
		public async Task<IActionResult> Update([FromBody] Employee employee)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var result = await _employeeService.UpdateEmployeeAsync(employee);

			if (!result.IsSuccess)
			{
				TempData["ErrorDetails"] = result.ErrorMessages;
				return BadRequest(result.ErrorMessages);
			}

			return Ok();
		}

		[HttpPost]
		public async Task<IActionResult> Delete([FromBody] Employee employee)
		{
			var result = await _employeeService.DeleteEmployeeAsync(employee);

			if (!result.IsSuccess)
			{
				TempData["ErrorDetails"] = result.ErrorMessages;
				return NotFound(result.ErrorMessages);
			}

			return Ok();
		}


		#region method summary
		// Method to handle both testing requirements (returning Ok/BadRequest for API clients) and
		// Redirecting to Index for browser form submissions
		#endregion
		private IActionResult HandleUploadResult(CsvImportResult result)
		{
			if (Request.IsAjaxRequest()) // Check for AJAX/API clients, for unit tests
			{
				return result.Errors.Any()
					? BadRequest(result)
					: Ok(result);
			}

			// For browser form submissions
			if (result.SuccessCount > 0)
			{
				TempData["Success"] = $"Successfully processed {result.SuccessCount} employees";
			}

			if (result.Errors.Any())
			{
				TempData["ErrorDetails"] = result.Errors;
			}

			return RedirectToAction("Index");
		}

	}
}
