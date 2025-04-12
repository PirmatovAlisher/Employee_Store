namespace EmployeeStore.Web.Extensions
{
	public static class HttpRequestExtensions
	{
		// Method to check for AJAX requests
		public static bool IsAjaxRequest(this HttpRequest request)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			// Check both headers
			return request.Headers != null
			&& request.Headers["X-Requested-With"] == "XMLHttpRequest";
		}
	}
}
