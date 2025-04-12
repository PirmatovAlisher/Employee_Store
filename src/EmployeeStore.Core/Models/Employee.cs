using System.ComponentModel.DataAnnotations;

namespace EmployeeStore.Core.Models
{
	public class Employee
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		[Display(Name = "Payroll Number")]
		public string PayrollNumber { get; set; }

		[Required]
		[Display(Name = "Forenames")]
		public string Forenames { get; set; }

		[Required]
		[Display(Name = "Surname")]
		public string Surname { get; set; }

		[Required]
		[Display(Name = "Date of Birth")]
		public string DateOfBirth { get; set; }

		[Display(Name = "Telephone")]
		public string Telephone { get; set; }

		[Display(Name = "Mobile")]
		public string Mobile { get; set; }

		[Display(Name = "Address")]
		public string Address { get; set; }

		[Display(Name = "Address 2")]
		public string Address2 { get; set; }

		[Display(Name = "Postcode")]
		public string Postcode { get; set; }

		[Display(Name = "Home Email")]
		[DataType(DataType.EmailAddress)]
		[EmailAddress(ErrorMessage = "Invalid email format")]
		public string EmailHome { get; set; }

		[Required]
		[Display(Name = "Start Date")]
		public string StartDate { get; set; }
	}
}
