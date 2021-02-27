using System;

namespace Routing.Api.Dto
{
    public class CompanyFullDto
    {
        public Guid Id { get; set; }
        public String Name { get; set; }
        public string Country { get; set; }
        public string Industry { get; set; }
        public string Product { get; set; }
        public string Introduction { get; set; }
        public DateTime? BankruptTime { get; set; }
    }
}
