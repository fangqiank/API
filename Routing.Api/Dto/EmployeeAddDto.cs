using Routing.Api.ValidationAttributes;

namespace Routing.Api.Dto
{
    [EmployeeNoDifferentFromFirstName(ErrorMessage = "员工编号必须和名不一样!!!")]
    public class EmployeeAddDto:EmployeeAddOrUpdate
    {
        
    }
}
