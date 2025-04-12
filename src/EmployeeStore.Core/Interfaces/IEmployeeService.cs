using EmployeeStore.Core.Models;

namespace EmployeeStore.Core.Interfaces
{
	public interface IEmployeeService
	{
		Task<CsvImportResult> ImportEmployeesAsync(IEnumerable<Employee> employees);
		IEnumerable<Employee> GetAllEmployees();
		Task<OperationResult<Employee>> DeleteEmployeeAsync(Employee employee);
		Task<OperationResult<Employee>> UpdateEmployeeAsync(Employee employee);
	}
}
