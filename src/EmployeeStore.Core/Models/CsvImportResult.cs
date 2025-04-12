namespace EmployeeStore.Core.Models
{
	public class CsvImportResult
	{
		public List<Employee> ValidEmployees { get; } = new List<Employee>();

		public int SuccessCount => ValidEmployees.Count;

		public List<string> Errors { get; set; } = new List<string>();
	}
}
