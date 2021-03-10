﻿namespace Routing.Api.Parameters
{
    //dto
    public class CompanyParameters
    {
        private const int MaxPageSize = 20;

        public string CompanyName { get; set; }

        public string SearchTerm { get; set; }

        public int PageNumber { get; set; } = 1;

        public string OrderBy { get; set; } = "CompanyName";

        public string Fields { get; set; }

        private int _pageSize = 10;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize: value;
        }

    }
}
