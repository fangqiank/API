using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Routing.Api.Dto;

namespace Routing.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class RootController : ControllerBase
    {
        [HttpGet(Name = nameof(GetRoot))]
        public IActionResult GetRoot()
        {
            var links = new List<LinkDto>
            {
                new LinkDto(
                    Url.Link(nameof(GetRoot), new { }),
                    "self",
                    "GET"),

                new LinkDto(
                    Url.Link(nameof(CompaniesController.GetCompanies), new { }),
                    "get companies",
                    "GET"),

                new LinkDto(
                    Url.Link(nameof(CompaniesController.CreateCompany), new { }),
                    "create a company",
                    "POST")
            };

            

            return Ok(links);
        }
    }
}
