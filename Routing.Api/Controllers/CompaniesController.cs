using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Routing.Api.Dto;
using Routing.Api.Entities;
using Routing.Api.Parameters;
using Routing.Api.Services;
using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Routing.Api.Helpers;

namespace Routing.Api.Controllers
{
    [ApiController]
    //[Route("api/[controller]")]
    [Route("api/companies")]
    public class CompaniesController:ControllerBase
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;

        public CompaniesController(ICompanyRepository companyRepository,IMapper mapper)
        {
            this._companyRepository = companyRepository ?? 
                                      throw new ArgumentException(nameof(companyRepository));
            this._mapper = mapper ?? throw new ArgumentException(nameof(mapper));
        }

        [HttpGet(Name = nameof(GetCompanies))]
        [HttpHead] //httphead返回body,但状态码也是200
        public async Task<ActionResult<IEnumerable<CompanyDto>>> GetCompanies(
                [FromQuery]CompanyParameters parameters) 
            //IActionResult可以用具体的实现类ActionResult<T>,返回的类型明确
        {
            var companies = await _companyRepository.GetCompaniesAsync(parameters);

            var previousLink = companies.HasPrevious
                ? CreateCompaniesRessourceUri(parameters, ResourceUriType.PreviousPage)
                : null;

            var nextLink = companies.HasNext
                ? CreateCompaniesRessourceUri(parameters, ResourceUriType.NextPage)
                : null;

            var paginationMetaData = new
            {
                totalCount = companies.TotalCount,
                pageSize = companies.PageSize,
                currentPage = companies.CurrentPage,
                totalPages=companies.TotalPages,
                previousPageLink =previousLink,
                nextPageLink =nextLink
            };

            Response.Headers.Add("X-Pagination",JsonSerializer.Serialize(paginationMetaData,new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }));
            //var companiesDto = new List<CompanyDto>();
            //replaced by Linq

            //foreach (var company in companies)
            //{
            //    companiesDto.Add(new CompanyDto
            //    {
            //        Id=company.Id,
            //        Name = company.Name
            //    });
            //}

            //companies.ToList().ForEach(company=>companiesDto.Add(new CompanyDto
            //{
            //    Id = company.Id,
            //    CompanyName  = company.Name
            //}));

            //Auto mapper
            var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);

           // return new JsonResult(companies);  //return Json

            return Ok(companiesDto);
        }

        [HttpGet("{companyId}",Name = nameof(GetCompany))] //controller route + companyId
        //[Route("{companyId}")]
        public async Task<ActionResult<CompanyDto>> GetCompany(Guid companyId)
        {
            var company = await _companyRepository.GetCompanyAsync(companyId);

            if (company == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CompanyDto>(company));
        }

        [HttpPost]
        public async Task<ActionResult<CompanyDto>> CreateCompany([FromBody]CompanyAddDto company)
        {
            //if (company == null)
            //{
            //    return BadRequest(); 400
            //}

            var entity = _mapper.Map<Company>(company);
            _companyRepository.AddCompany(entity);
            await _companyRepository.SaveAsync();

            var returnDto = _mapper.Map<CompanyDto>(entity);

            return CreatedAtAction(nameof(GetCompany), 
                new {companyId = returnDto.Id}, returnDto);
        }

        [HttpDelete("{companyId}")]
        //DBContext中设置级联：OnDelete(DeleteBehavior.Cascade)，删除记录后，其子记录也被删除
        public async Task<IActionResult> DeleteCompany(Guid companyId)
        {
            var companyEntity = await _companyRepository.GetCompanyAsync(companyId);

            if (companyEntity == null)
            {
                return NotFound();
            }

            await _companyRepository.GetEmployeesAsync(companyId, null);

            _companyRepository.DeleteCompany(companyEntity);
            await _companyRepository.SaveAsync();

            return NoContent();
        }


        [HttpOptions]
        public IActionResult GetCompaniesOptions()
        {
            Response.Headers.Add("Allow", "GET,POST,PATCH,OPTIONS");
            return Ok();
        }

        private string CreateCompaniesRessourceUri(CompanyParameters parameters,ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link(nameof(GetCompanies), new
                    {
                        pageNumber = parameters.PageNumber - 1,
                        pageSize = parameters.PageSize,
                        companyName = parameters.CompanyName,
                        searchTerm = parameters.SearchTerm
                    });

                case ResourceUriType.NextPage:
                    return Url.Link(nameof(GetCompanies), new
                    {
                        pageNumber = parameters.PageNumber + 1,
                        pageSize = parameters.PageSize,
                        companyName = parameters.CompanyName,
                        searchTerm = parameters.SearchTerm
                    });

                default:
                    return Url.Link(nameof(GetCompanies), new
                    {
                        pageNumber = parameters.PageNumber,
                        pageSize = parameters.PageSize,
                        companyName = parameters.CompanyName,
                        searchTerm = parameters.SearchTerm
                    });
            }
        }
    }
}
