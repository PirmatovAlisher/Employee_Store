namespace EmployeeStore.Core.Models
{
	public class OperationResult<T>
	{
		public bool IsSuccess { get; set; }

		public List<string> ErrorMessages { get; set; } = new List<string>();
	}
}
