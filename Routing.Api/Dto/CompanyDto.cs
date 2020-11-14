using System;

namespace Routing.Api.Dto
{
    public class CompanyDto
    {
        public Guid Id{ get; set; }
        public String  CompanyName { get; set; }
        public string Country { get; set; }
        public string Industry { get; set; }
        public string Product { get; set; }
        public string Introduction { get; set; }
    }
}
