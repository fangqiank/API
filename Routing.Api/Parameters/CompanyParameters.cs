using System;

namespace Routing.Api.Parameters
{
    public class CompanyParameters
    {
        private const int MaxPageSize = 20;

        public string CompanyName { get; set; }

        public string SearchTerm { get; set; }

        public int PageNumber { get; set; } = 1;

        private int _pageSize = 5;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize: value;
        }

    }
}
