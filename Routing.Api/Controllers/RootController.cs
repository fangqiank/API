using Microsoft.AspNetCore.Mvc;
using Routing.Api.Dto;
using System.Collections.Generic;

namespace Routing.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class RootController : ControllerBase //根文档
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
