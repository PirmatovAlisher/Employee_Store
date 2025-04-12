using EmployeeStore.Core.Models;

namespace EmployeeStore.Core.Interfaces
{
	public interface ICsvParserService
	{
		CsvImportResult ParseEmployees(Stream fileStream);
	}
}
