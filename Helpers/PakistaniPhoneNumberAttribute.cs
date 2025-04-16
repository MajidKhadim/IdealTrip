using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace IdealTrip.Helpers
{
	public class PakistaniPhoneNumberAttribute :ValidationAttribute
	{
		public PakistaniPhoneNumberAttribute()
		{
		}
			protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			string phoneNumber = value as string;

			if (string.IsNullOrWhiteSpace(phoneNumber))
				return new ValidationResult("Phone number is required.");

			// Accept numbers like 03XXXXXXXXX or +923XXXXXXXXX
			var isValid = Regex.IsMatch(phoneNumber, @"^(03\d{9}|\+923\d{9})$");

			if (!isValid)
				return new ValidationResult("Phone number must be a valid Pakistani mobile number (e.g., 03XXXXXXXXX or +923XXXXXXXXX).");

			return ValidationResult.Success;
		}
	}
}
