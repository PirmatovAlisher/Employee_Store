using CsvHelper.Configuration;
using EmployeeStore.Core.Models;

namespace EmployeeStore.Services.CsvMappings
{
	public class EmployeeCsvMap : ClassMap<Employee>
	{
		public EmployeeCsvMap()
		{
			Map(m => m.PayrollNumber).Name("Personnel_Records.Payroll_Number");
			Map(m => m.Forenames).Name("Personnel_Records.Forenames");
			Map(m => m.Surname).Name("Personnel_Records.Surname");
			Map(m => m.DateOfBirth).Name("Personnel_Records.Date_of_Birth");
			Map(m => m.Telephone).Name("Personnel_Records.Telephone");
			Map(m => m.Mobile).Name("Personnel_Records.Mobile");
			Map(m => m.Address).Name("Personnel_Records.Address");
			Map(m => m.Address2).Name("Personnel_Records.Address_2");
			Map(m => m.Postcode).Name("Personnel_Records.Postcode");
			Map(m => m.EmailHome).Name("Personnel_Records.EMail_Home")
				.Convert(args => args.Row.GetField("Personnel_Records.EMail_Home")?.Trim().ToLower());
			Map(m => m.StartDate).Name("Personnel_Records.Start_Date");
		}
	}
}
