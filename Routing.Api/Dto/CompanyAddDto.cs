using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Routing.Api.Dto
{
    //针对查询，创建，编辑建立相应的Dto
    public class CompanyAddDto
    {
        [Display(Name = "名称")]
        [Required(ErrorMessage = "{0}这个字段是必填的")]
        [MaxLength(100,ErrorMessage = "{0}的最大长度不超过{1}")]
        public string Name { get; set; }

        [Display(Name = "简介")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "{0}的长度范围从{2}到{1}")]
        public string Introduction { get; set; }

        public ICollection<EmployeeAddDto> Employees { get; set; }=new List<EmployeeAddDto>(); 
        //employees和entity中的导航属性employees名称一致
    }
}
