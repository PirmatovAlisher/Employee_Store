using EmployeeStore.Core.Models;

namespace EmployeeStore.Core.Interfaces
{
	public interface IEmployeeRepository
	{
		Task AddEmployeesAsync(IEnumerable<Employee> employees);
		IQueryable<Employee> GetAllEmployees();
		Task<Employee?> GetEmployeeByIdAsync(Guid id);
		Task<bool> UpdateEmployeeAsync(Employee employee);
		Task DeleteEmployeeAsync(Employee employee);
	}
}
