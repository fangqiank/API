using System;

namespace Routing.Api.Dto
{
    public class CompanyAddWithBankruptTimeDto:CompanyAddDto
    {
        public DateTime BankruptTime { get; set; }
    }
}
