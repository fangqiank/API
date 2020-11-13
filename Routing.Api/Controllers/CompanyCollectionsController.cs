using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Routing.Api.Dto;
using Routing.Api.Entities;
using Routing.Api.Helpers;
using Routing.Api.Services;

namespace Routing.Api.Controllers
{
    [ApiController]
    [Route("api/companycollections")]
    public class CompanyCollectionsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ICompanyRepository _companyRepository;

        public CompanyCollectionsController(IMapper mapper,ICompanyRepository companyRepository)
        {
            this._mapper = mapper ?? throw new ArgumentException(nameof(mapper));
            this._companyRepository = companyRepository ?? throw new ArgumentException(nameof(companyRepository));
        }

        [HttpGet("({ids})",Name = nameof(GetCompanyCollection))]
        public async Task<IActionResult> GetCompanyCollection(
            [FromRoute]
            [ModelBinder(BinderType=typeof(ArrayModelBinder))]
            IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            var entities = await _companyRepository.GetCompaniesAsync(ids);

            if (ids.Count() != entities.Count())
            {
                return NotFound();
            }

            var dtosReturn = _mapper.Map<IEnumerable<CompanyDto>>(entities);

            return Ok(dtosReturn);
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<CompanyDto>>> 
            CreateCompanyCollection(IEnumerable<CompanyAddDto> companyCollection)
        {
            var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCollection);

            companyEntities.ToList().ForEach(x => _companyRepository.AddCompany(x));

            await _companyRepository.SaveAsync();

            var dtosReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);

            var idsString = string.Join(",", dtosReturn.Select(
                x => x.Id));

            return CreatedAtAction(nameof(GetCompanyCollection),new{ids =idsString}, dtosReturn);

        }

        
    }
}
