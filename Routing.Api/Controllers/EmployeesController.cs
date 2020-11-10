using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Routing.Api.Dto;
using Routing.Api.Entities;
using Routing.Api.Services;

namespace Routing.Api.Controllers
{
    [ApiController]
    [Route("api/companies/{companyId}/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ICompanyRepository _companyRepository;

        public EmployeesController(IMapper mapper,ICompanyRepository companyRepository)
        {
            this._mapper = mapper ?? throw  new ArgumentException(nameof(mapper));
            this._companyRepository = companyRepository ?? throw new ArgumentException(nameof(companyRepository));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> 
            GetEmployeesForCompany(Guid companyId,[FromQuery(Name = "gender")]string genderDisplay,
                string q) 
            //[FromQuery(Name = "gender")] Query String
        {
            if (! await _companyRepository.CompanyExistsAsync(companyId))
            {
                return NotFound();
            }

            var employees = await _companyRepository.GetEmployeesAsync(companyId,genderDisplay,q);

            var employeeDtos = _mapper.Map<IEnumerable<EmployeeDto>>(employees);

            return Ok(employeeDtos);
        }

        [HttpGet("{employeeId}",Name = "nameof(GetEmployeeForCompany)")]
        public async Task<ActionResult<EmployeeDto>> 
            GetEmployeeForCompany(Guid companyId,Guid employeeId)
        {
            if (!await _companyRepository.CompanyExistsAsync(companyId))
            {
                return NotFound();
            }

            var employee = await _companyRepository.GetEmployeeAsync(companyId,employeeId);

            if(employee==null)
            {
                return NotFound();
            }
            

            var employeeDto = _mapper.Map<EmployeeDto>(employee);

            return Ok(employeeDto);
        }

        [HttpPost]
        public async Task<ActionResult<EmployeeDto>> 
            CreateEmployeeForCompany(Guid companyId,EmployeeAddDto employee)
        {
            if (!await _companyRepository.CompanyExistsAsync(companyId))
            {
                return NotFound();
            }

            var entity = _mapper.Map<Employee>(employee);

            _companyRepository.AddEmployee(companyId, entity);

            await _companyRepository.SaveAsync();

            var dtoReturn = _mapper.Map<EmployeeDto>(entity);

            //CreatedAtRoute?return 500
            return CreatedAtAction(nameof(GetEmployeeForCompany), new
            {
                companyId = companyId,
                employeeId = dtoReturn.Id

            }, dtoReturn);
            
        }
    }

    
}
