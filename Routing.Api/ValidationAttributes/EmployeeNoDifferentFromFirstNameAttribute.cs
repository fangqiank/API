using System.ComponentModel.DataAnnotations;
using Routing.Api.Dto;

namespace Routing.Api.ValidationAttributes
{
    public class EmployeeNoDifferentFromFirstNameAttribute:ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var addDto = (EmployeeAddDto) validationContext.ObjectInstance;

            if (addDto.EmployeeNo == addDto.FirstName)
            {
                return new ValidationResult(ErrorMessage, new []{nameof(EmployeeAddDto)});
            }

            return ValidationResult.Success;
        }
    }
}
