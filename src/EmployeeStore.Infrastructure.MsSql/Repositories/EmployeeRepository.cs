using EmployeeStore.Core.Interfaces;
using EmployeeStore.Core.Models;
using EmployeeStore.Infrastructure.MsSql.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeStore.Infrastructure.MsSql.Repositories
{
	public class EmployeeRepository : IEmployeeRepository
	{

		private readonly EmployeeDbContext _context;

		public EmployeeRepository(EmployeeDbContext context)
		{
			_context = context;
		}

		public async Task AddEmployeesAsync(IEnumerable<Employee> employees)
		{
			await _context.Employees.AddRangeAsync(employees);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteEmployeeAsync(Employee employee)
		{
			_context.Employees.Remove(employee);
			await _context.SaveChangesAsync();
		}

		public IQueryable<Employee> GetAllEmployees()
		{
			var employees = _context.Employees.AsQueryable();
			return employees;
		}

		public async Task<Employee?> GetEmployeeByIdAsync(Guid id)
		{
			var employee = await _context.Employees.FirstOrDefaultAsync(x => x.Id == id);
			return employee;
		}

		public async Task<bool> UpdateEmployeeAsync(Employee employee)
		{
			var employeeFromDb = await GetEmployeeByIdAsync(employee.Id);

			if (employeeFromDb != null)
			{
				employeeFromDb.PayrollNumber = employee.PayrollNumber;
				employeeFromDb.Forenames = employee.Forenames;
				employeeFromDb.Surname = employee.Surname;
				employeeFromDb.DateOfBirth = employee.DateOfBirth;
				employeeFromDb.Mobile = employee.Mobile;
				employeeFromDb.Telephone = employee.Telephone;
				employeeFromDb.Address = employee.Address;
				employeeFromDb.Address2 = employee.Address2;
				employeeFromDb.Postcode = employee.Postcode;
				employeeFromDb.EmailHome = employee.EmailHome;
				employeeFromDb.StartDate = employee.StartDate;


				await _context.SaveChangesAsync();
				return true;
			}

			return false;
		}
	}
}
