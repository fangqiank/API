using System;
using System.ComponentModel.DataAnnotations;
using Routing.Api.Dto;

namespace Routing.Api.ValidationAttributes
{
    public class EmployeeNoDifferentFromFirstNameAttribute:ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var addDto = (EmployeeAddOrUpdate) validationContext.ObjectInstance;

            return addDto.EmployeeNo.Equals(addDto.FirstName,StringComparison.OrdinalIgnoreCase) ? 
                new ValidationResult(ErrorMessage, new []{nameof(EmployeeAddOrUpdate) }) 
                : ValidationResult.Success;
        }
    }
}
