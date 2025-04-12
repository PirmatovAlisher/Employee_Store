using CsvHelper;
using CsvHelper.Configuration;
using EmployeeStore.Core.Interfaces;
using EmployeeStore.Core.Models;
using EmployeeStore.Services.CsvMappings;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace EmployeeStore.Services.EmployeeServices
{
	public class CsvParserService : ICsvParserService
	{
		public CsvImportResult ParseEmployees(Stream fileStream)
		{

			var result = new CsvImportResult();
			var employees = new List<Employee>();

			var config = new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				MissingFieldFound = null,
				BadDataFound = context => result.Errors.Add($"Bad data in row {context.Context.Parser!.Row}: {context.RawRecord}")
			};

			using var reader = new StreamReader(fileStream);
			using var csv = new CsvReader(reader, config);

			csv.Context.RegisterClassMap<EmployeeCsvMap>();

			while (csv.Read())
			{
				try
				{
					var record = csv.GetRecord<Employee>();

					// Validating entity
					var validationResult = new List<ValidationResult>();
					if (!Validator.TryValidateObject(record, new ValidationContext(record), validationResult))
					{
						result.Errors.Add($"Invalid data in row: {csv.Parser.Row}");
						result.Errors.AddRange(validationResult.Select(x => x.ErrorMessage)!);
						if (!EmailValidation(record))
						{
							result.Errors.Add($"Invalid Email format \"{record.EmailHome}\" in row: {csv.Parser.Row}");
						}
						continue;
					}
					// Email validation
					if (!EmailValidation(record))
					{
						result.Errors.Add($"Invalid Email format \"{record.EmailHome}\" in row: {csv.Parser.Row}");
						continue;
					}

					employees.Add(record);
				}
				catch (Exception ex)
				{
					result.Errors.Add($"CSV parsing failed in row {csv.Parser.Row}: {ex.Message}");
				}
			}
			#region previous iteration logic

			//try
			//{
			//	var records = csv.GetRecords<Employee>();

			//	foreach (var record in records)
			//	{
			//		try
			//		{
			//			// Validating entity
			//			var validationResult = new List<ValidationResult>();
			//			if (!Validator.TryValidateObject(record, new ValidationContext(record), validationResult))
			//			{
			//				result.Errors.Add($"Invalid data in row: {csv.Parser.Row}");
			//				result.Errors.AddRange(validationResult.Select(x => x.ErrorMessage)!);
			//				if (!EmailValidation(record))
			//				{
			//					result.Errors.Add($"Invalid Email format \"{record.EmailHome}\" in row: {csv.Parser.Row}");
			//				}
			//				continue;
			//			}
			//			// Email validation
			//			if (!EmailValidation(record))
			//			{
			//				result.Errors.Add($"Invalid Email format \"{record.EmailHome}\" in row: {csv.Parser.Row}");
			//				continue;
			//			}

			//			employees.Add(record);
			//		}
			//		catch (Exception ex)
			//		{
			//			result.Errors.Add($"Row {csv.Parser.Row}: {ex.Message}");
			//			continue;
			//		}
			//	}
			//}
			//catch (Exception ex)
			//{
			//	result.Errors.Add($"CSV parsing failed in row {csv.Parser.Row}: {ex.Message}");
			//}
			#endregion

			result.ValidEmployees.AddRange(employees);

			return result;
		}

		private bool EmailValidation(Employee employee)
		{
			if (!string.IsNullOrWhiteSpace(employee.EmailHome))
			{
				var emailParts = employee.EmailHome.Split('@');
				if (emailParts.Length != 2 || emailParts[1].Count(c => c == '.') < 1)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			return false;
		}
	}
}
