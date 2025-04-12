using EmployeeStore.Core.Interfaces;
using EmployeeStore.Core.Models;
using System.Data.Common;

namespace EmployeeStore.Services.EmployeeServices
{
	public class EmployeeService : IEmployeeService
	{
		private readonly IEmployeeRepository _repository;

		public EmployeeService(IEmployeeRepository repository)
		{
			_repository = repository;
		}

		public async Task<CsvImportResult> ImportEmployeesAsync(IEnumerable<Employee> employees)
		{

			var result = new CsvImportResult();
			var validEmployees = new List<Employee>();

			// Get all payrollNumber from incoming data
			var payrollNumbers = employees.Select(x => x.PayrollNumber).Distinct().ToList();
			var existingNumber = _repository.GetAllEmployees().Where(x => payrollNumbers.Contains(x.PayrollNumber))
				.Select(e => e.PayrollNumber).ToList();

			foreach (var employee in employees)
			{
				try
				{
					// Checking DB duplication 
					if (existingNumber.Contains(employee.PayrollNumber))
					{
						result.Errors.Add($"Payroll number {employee.PayrollNumber} already exists in system");
						continue;
					}

					validEmployees.Add(employee);
				}
				catch (Exception ex)
				{
					result.Errors.Add($"Error occurred while saving an employee with Payroll number: {employee.PayrollNumber} \nError details: {ex.Message}");
				}
			}

			try
			{
				await _repository.AddEmployeesAsync(validEmployees);
				result.ValidEmployees.AddRange(validEmployees);
			}
			catch (DbException ex)
			{
				result.Errors.Add($"Database error: {ex.InnerException?.Message}");
			}

			return result;

		}

		public IEnumerable<Employee> GetAllEmployees()
		{
			//Data sorted by surname ascending
			var employees = _repository.GetAllEmployees().OrderBy(x => x.Surname).ToList();

			return employees;
		}

		public async Task<OperationResult<Employee>> DeleteEmployeeAsync(Employee employee)
		{
			var result = new OperationResult<Employee>();

			try
			{
				var employeeFromDb = await _repository.GetEmployeeByIdAsync(employee.Id);

				if (employeeFromDb == null)
				{
					result.IsSuccess = false;
					result.ErrorMessages.Add("Employee not found");
					return result;
				}
				else
				{
					await _repository.DeleteEmployeeAsync(employeeFromDb);
					result.IsSuccess = true;
					return result;
				}
			}
			catch (Exception ex)
			{
				result.IsSuccess = false;
				result.ErrorMessages
					.Add($"An error occurred while deleting the employee with Payroll number \"{employee.PayrollNumber}\" : {ex.Message}");
			}
			return result;
		}

		public async Task<OperationResult<Employee>> UpdateEmployeeAsync(Employee employee)
		{
			var result = new OperationResult<Employee>();

			try
			{
				var isUpdateSuccess = await _repository.UpdateEmployeeAsync(employee);

				if (!isUpdateSuccess)
				{
					result.IsSuccess = false;
					result.ErrorMessages.Add("Employee not found");
					return result;
				}
				else
				{
					result.IsSuccess = true;
				}
			}
			catch (Exception ex)
			{
				result.IsSuccess = false;
				result.ErrorMessages
					.Add($"An error occurred while updating the employee with Payroll number \"{employee.PayrollNumber}\" : {ex.Message}");
			}

			return result;
		}
	}
}
