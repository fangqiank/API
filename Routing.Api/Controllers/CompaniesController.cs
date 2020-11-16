using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Routing.Api.Dto;
using Routing.Api.Entities;
using Routing.Api.Helpers;
using Routing.Api.Parameters;
using Routing.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Routing.Api.ActionConstraints;

namespace Routing.Api.Controllers
{
    [ApiController]
    //[Route("api/[controller]")]
    [Route("api/companies")]
    public class CompaniesController:ControllerBase
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckService _propertyCheckService;

        public CompaniesController(ICompanyRepository companyRepository,IMapper mapper,
            IPropertyMappingService propertyMappingService, IPropertyCheckService propertyCheckService)
        {
            this._companyRepository = companyRepository ?? 
                                      throw new ArgumentNullException(nameof(companyRepository));
            this._mapper = mapper ?? 
                           throw new ArgumentNullException(nameof(mapper));
            this._propertyMappingService = propertyMappingService??
                                           throw new ArgumentNullException(nameof(propertyMappingService));
            this._propertyCheckService = propertyCheckService ??
                throw  new ArgumentNullException(nameof(propertyCheckService));
        }

        [HttpGet(Name = nameof(GetCompanies))]
        [HttpHead] //httphead返回body,但状态码也是200
        //public async Task<ActionResult<IEnumerable<CompanyDto>>> GetCompanies(
        public async Task<IActionResult> GetCompanies([FromQuery]CompanyParameters parameters) 
            //IActionResult可以用具体的实现类ActionResult<T>,返回的类型明确
        {
            
            //return 400
            if (!_propertyMappingService.ValidMappingExistsFor<CompanyDto, Company>(parameters.orderBy))
            {
                return BadRequest();
            }

            //return 400
            if (!_propertyCheckService.TypeHasProperties<CompanyDto>(parameters.Fields))
            {
                return BadRequest();
            }

            var companies = await _companyRepository.GetCompaniesAsync(parameters);

            //var previousLink = companies.HasPrevious
            //    ? CreateCompaniesRessourceUri(parameters, ResourceUriType.PreviousPage)
            //    : null;

            //var nextLink = companies.HasNext
            //    ? CreateCompaniesRessourceUri(parameters, ResourceUriType.NextPage)
            //    : null;

            var paginationMetaData = new
            {
                totalCount = companies.TotalCount,
                pageSize = companies.PageSize,
                currentPage = companies.CurrentPage,
                totalPages=companies.TotalPages,
                //previousPageLink =previousLink,
                //nextPageLink =nextLink
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

            

            var companiesDtos = _mapper.Map<IEnumerable<CompanyDto>>(companies);

            var shapedData = companiesDtos.ShapeData(parameters.Fields);
            // return new JsonResult(companies);  //return Json

            
            var links = CreateLinksForCompany(parameters, companies.HasPrevious,
                companies.HasNext);

            //对于集合资源，返回对象有这些属性
            //{value:[xxx]集合,links

            var shapedCompaniesWithLinks = shapedData.Select(c =>
            {
                var companyDict = c as IDictionary<string, object>;
                var companyLinks = CreateLinksForCompany(
                    (Guid)companyDict["Id"],
                    null);
                companyDict.Add("links", companyLinks);
                return companyDict;
            });


            var linkedCollectionResource = new
            {
                value = shapedCompaniesWithLinks,
                links = links
            };

            return Ok(linkedCollectionResource);
        }

        //content type用于该action,full/friendly针对于companyDto/companyFullDto(字段的差别),
        //hateoas返回带链接的
        [Produces("application/json",
        "application/vnd.company.hateoas+json",
        "application/vnd.company.company.friendly+json",
        "application/vnd.company.company.friendly.hateoas+json",
        "application/vnd.company.company.full+json",
        "application/vnd.company.company.full.hateoas+json")]
        [HttpGet("{companyId}",Name = nameof(GetCompany))] //controller route + companyId
        //[Route("{companyId}")]
        public async Task<ActionResult<CompanyDto>> GetCompany(Guid companyId,string fields,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            //Vendor-specific media types
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            //400 bad request
            if (!_propertyCheckService.TypeHasProperties<CompanyDto>(fields))
            {
                return BadRequest();
            }

            var company = await _companyRepository.GetCompanyAsync(companyId);

            if (company == null)
            {
                return NotFound();
            }

            var includeLinks = parsedMediaType.SubTypeWithoutSuffix
                .EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);

            IEnumerable<LinkDto> myLinks = new List<LinkDto>();

            if (includeLinks)
            {
                myLinks = CreateLinksForCompany(companyId, fields);
            }

           
            var primaryMediaType = includeLinks
                ? parsedMediaType.SubTypeWithoutSuffix.Substring(0,
                    parsedMediaType.SubTypeWithoutSuffix.Length - "hateoas.".Length)
                : parsedMediaType.SubTypeWithoutSuffix;

            if (primaryMediaType == "vnd.company.company.full")
            {
                var full = _mapper.Map<CompanyFullDto>(company)
                    .ShapeData(fields) as IDictionary<string, object>;
                if (includeLinks)
                {
                    full.Add("links",myLinks);
                }

                return Ok(full);
            }

            var friendly = _mapper.Map<CompanyDto>(company)
                .ShapeData(fields) as IDictionary<string, object>;

            if (includeLinks)
            {
                friendly.Add("links",myLinks);
            }

            return Ok(friendly);


            //if (parsedMediaType.MediaType == "application/vnd.company.hateoas+json")
            //{
            //    var links = CreateLinksForCompany(companyId, fields); //build links(HATEOS)

            //    var linkedDict = _mapper.Map<CompanyDto>(company)
            //        .ShapeData(fields) as IDictionary<string, object>;

            //    linkedDict.Add("links", links);

            //    return Ok(linkedDict);
            //}

            //return Ok(_mapper.Map<CompanyDto>(company).ShapeData(fields) as IDictionary<string, object>);

        }

        [HttpPost(Name = nameof(CreateCompanyAddWithBankruptTime))]
        [RequestHeaderMatchesMediaType("Content-Type",
            "application/vnd.company.company.companyforcreationwithbankcrupttime+json")]
        [Consumes("application/vnd.company.company.companyforcreationwithbankcrupttime+json")]
        public async Task<ActionResult<CompanyDto>> CreateCompanyAddWithBankruptTime([FromBody] CompanyAddWithBankruptTimeDto company)
        {
            var entity = _mapper.Map<Company>(company);
            _companyRepository.AddCompany(entity);
            await _companyRepository.SaveAsync();

            var returnDto = _mapper.Map<CompanyDto>(entity);

            var link = CreateLinksForCompany(returnDto.Id, null);

            var linkDict = returnDto.ShapeData(null)
                as IDictionary<string, object>;

            linkDict.Add("links", link);

            return CreatedAtAction(nameof(GetCompany),
                //new {companyId = returnDto.Id}, returnDto);
                new { companyId = linkDict["Id"] },
                linkDict);
        }

        [HttpPost(Name = nameof(CreateCompany))]
        [RequestHeaderMatchesMediaType("Content-Type","application/json",
            "application/vnd.company.company.companyforcreation+json")]
        [Consumes("application/json", 
            "application/vnd.company.company.companyforcreation+json")]
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

            var link = CreateLinksForCompany(returnDto.Id, null);

            var linkDict = returnDto.ShapeData(null)
                as IDictionary<string, object>;

            linkDict.Add("links",link);

            return CreatedAtRoute(nameof(GetCompany),
                //new {companyId = returnDto.Id}, returnDto);
                new {companyId = linkDict["Id"]},
                linkDict);
        }


        [HttpDelete("{companyId}",Name = nameof(DeleteCompany))]
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
                        fields= parameters.Fields,
                        orderBy =parameters.orderBy,
                        pageNumber = parameters.PageNumber - 1,
                        pageSize = parameters.PageSize,
                        companyName = parameters.CompanyName,
                        searchTerm = parameters.SearchTerm
                    });

                case ResourceUriType.NextPage:
                    return Url.Link(nameof(GetCompanies), new
                    {
                        fields = parameters.Fields,
                        orderBy = parameters.orderBy,
                        pageNumber = parameters.PageNumber + 1,
                        pageSize = parameters.PageSize,
                        companyName = parameters.CompanyName,
                        searchTerm = parameters.SearchTerm
                    });
                        
                default:
                    return Url.Link(nameof(GetCompanies), new
                    {
                        fields = parameters.Fields,
                        orderBy = parameters.orderBy,
                        pageNumber = parameters.PageNumber,
                        pageSize = parameters.PageSize,
                        companyName = parameters.CompanyName,
                        searchTerm = parameters.SearchTerm
                    });
            }
        }

        //HATEOS
        private IEnumerable<LinkDto> CreateLinksForCompany(Guid companyId,string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(new LinkDto(
                    Url.Link(nameof(GetCompany), new { companyId = companyId }),
                    "self",
                    "GET"));
            }
            else
            {
                links.Add(new LinkDto(
                    Url.Link(nameof(GetCompany), new { companyId = companyId, fields = fields }),
                    "self",
                    "GET"));
            }

            links.Add(new LinkDto(
                Url.Link(nameof(DeleteCompany), new { companyId = companyId}),
                "delete company",
                "DELETE"));

            links.Add(new LinkDto(
                Url.Link(nameof(EmployeesController.CreateEmployeeForCompany), new { companyId = companyId }),
                "create a employee for company",
                "POST"));

            links.Add(new LinkDto(
                Url.Link(nameof(EmployeesController.GetEmployeesForCompany), new { companyId = companyId }),
                    "employees",
                    "GET"));
            

            return links;
        }

        //Getcompanies建立链接
        private IEnumerable<LinkDto> CreateLinksForCompany(CompanyParameters parameters,
            bool hasPrevious,bool hasNext)
        {
            var links = new List<LinkDto>();

            links.Add(new LinkDto(
                CreateCompaniesRessourceUri(parameters,ResourceUriType.CurrentPage),
                "self",
                "GET"
                ));

            if(hasPrevious)
                links.Add(new LinkDto(
                    CreateCompaniesRessourceUri(parameters, ResourceUriType.PreviousPage),
                    "previous page",
                    "GET"
                ));

            if (hasNext)
                links.Add(new LinkDto(
                    CreateCompaniesRessourceUri(parameters, ResourceUriType.NextPage),
                    "next page",
                    "GET"
                ));

            return links;
        }


    }
}
