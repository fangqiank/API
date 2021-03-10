using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Routing.Api.Dto;
using Routing.Api.Entities;
using Routing.Api.Parameters;
using Routing.Api.Services;

namespace Routing.Api.Controllers
{
    [ApiController]
    [Route("api/companies/{companyId}/employees")]
    //[ResponseCache(CacheProfileName = "120sCacheProfile")]  //controller缓存，CacheProfileName在startup里定义
    [HttpCacheExpiration(CacheLocation = CacheLocation.Public)]
    [HttpCacheValidation(MustRevalidate = true)]
    public class EmployeesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ICompanyRepository _companyRepository;

        public EmployeesController(IMapper mapper,ICompanyRepository companyRepository)
        {
            this._mapper = mapper ?? throw  new ArgumentNullException(nameof(mapper));
            this._companyRepository = companyRepository ?? 
                                      throw new ArgumentNullException(nameof(companyRepository));
        }



        [HttpGet(Name = nameof(GetEmployeesForCompany))]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>>
            //GetEmployeesForCompany(Guid companyId,[FromQuery(Name = "gender")]string genderDisplay,
            //    string q) 
            GetEmployeesForCompany(Guid companyId, [FromQuery] EmployeeParameters parameters)

        //[FromQuery(Name = "gender")] Query String
        {
            if (!await _companyRepository.CompanyExistsAsync(companyId))
            {
                return NotFound();
            }

            //var employees = await _companyRepository.GetEmployeesAsync(companyId,genderDisplay,q);
            var employees = await _companyRepository.GetEmployeesAsync(companyId, parameters);

            var employeeDtos = _mapper.Map<IEnumerable<EmployeeDto>>(employees);

            return Ok(employeeDtos);
        }

        [HttpGet("{employeeId}",Name = "nameof(GetEmployeeForCompany)")]
        //[ResponseCache(Duration = 60)] //action缓存

        //third-party, marvin.cache.header
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public,MaxAge = 1800)]
        [HttpCacheValidation(MustRevalidate = false)]
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

        [HttpPost(Name = nameof(CreateEmployeeForCompany))]
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
                companyId,
                employeeId = dtoReturn.Id

            }, dtoReturn);
            
        }

        [HttpPut("{employeeId}")] //put整体替换，patch局部更新
        //如果employeeId存在，做更新操作;不存在，则添加新的employee记录
        public async Task<ActionResult<EmployeeDto>> UpdateEmployeeForCompany(Guid companyId,
            Guid employeeId,EmployeeUpdatedDto employee)
        {
            if (!await _companyRepository.CompanyExistsAsync(companyId))
            {
                return NotFound();
            }

            var employeeEntity = await _companyRepository.GetEmployeeAsync(companyId, employeeId);

            if (employeeEntity == null)
            {
                var employeeToAddEntity = _mapper.Map<Employee>(employee);
                employeeToAddEntity.Id = employeeId;
                
                _companyRepository.AddEmployee(companyId,employeeToAddEntity);
                await _companyRepository.SaveAsync();

                var dtoReturn = _mapper.Map<EmployeeDto>(employeeToAddEntity);

                return CreatedAtAction(nameof(GetEmployeeForCompany), new
                {
                    companyId = companyId,
                    employeeId = dtoReturn.Id
                }, dtoReturn);
            }

            //entity转换为updateDto
            //把传进来的employee的值更新到updateDto
            //把updateDto映射回entity
            _mapper.Map(employee, employeeEntity);

            _companyRepository.UpdateEmployee(employeeEntity);

            await _companyRepository.SaveAsync();

            return Ok(employeeEntity);  //or return NoContent() -status code:204

        }

        
        [HttpPatch("{employeeId}")]

        #region json patch operations
            //operations:
            //Add:{"op","add","path":"/biscuits/1", "value":{"name":"Ginger nut"}}
            //Remove:{"op":"remove","path":"/biscuits"}
            //Move:{"op":"move","from":"/biscuits","path":"/cookies"}
            //Replace:{"op":"replace","path":"/biscuits/0/name","value":"Chocolate digestive"}
            //Copy:{"op":"copy","from":"/biscuits/0","path":"/best_biscuit"}
            //Test:{"op":"test","path":"/best_biscuit/name","value":"Choco Leibniz"}
        #endregion

        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(
            Guid companyId,
            Guid employeeId,
            JsonPatchDocument<EmployeeUpdatedDto> patchDocument)
        {
            if (!await _companyRepository.CompanyExistsAsync(companyId))
            {
                return NotFound();
            }

            var employeeEntity = await _companyRepository.GetEmployeeAsync(companyId, employeeId);

            if (employeeEntity == null)
            {
                var employeeDto = new EmployeeUpdatedDto();
                patchDocument.ApplyTo(employeeDto,ModelState);

                if (!TryValidateModel(employeeDto))
                {
                    return ValidationProblem(ModelState);
                }

                var employeeToAdd = _mapper.Map<Employee>(employeeDto);
                employeeToAdd.Id = employeeId;

                _companyRepository.AddEmployee(companyId, employeeToAdd);
                await _companyRepository.SaveAsync();

                var dtoToReturn = _mapper.Map<EmployeeDto>(employeeToAdd);

                return CreatedAtAction(nameof(GetEmployeeForCompany), new
                {
                    companyId,
                    employeeId = dtoToReturn.Id
                }, dtoToReturn);
            }

            var dtoToPatch = _mapper.Map<EmployeeUpdatedDto>(employeeEntity);

            // 需要处理验证错误
            patchDocument.ApplyTo(dtoToPatch,ModelState);

            if (!TryValidateModel(dtoToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(dtoToPatch, employeeEntity);

            _companyRepository.UpdateEmployee(employeeEntity);

            await _companyRepository.SaveAsync();

            return Ok(dtoToPatch);
        }

        [HttpDelete("{employeeId}")]
        public async Task<IActionResult> DeleteEmploeeForCompany(Guid companyId, Guid employeeId)
        {
            if (!await _companyRepository.CompanyExistsAsync((companyId)))
            {
                return NotFound();
            }

            var employeeEntity = await _companyRepository.GetEmployeeAsync(companyId, employeeId);

            if (employeeEntity == null)
            {
                return NotFound();
            }

            _companyRepository.DeleteEmployee(employeeEntity);

            await _companyRepository.SaveAsync();

            return NoContent();
        }

        //运用自定义验证报告
        public override ActionResult ValidationProblem(ModelStateDictionary modelStateDictionary)
        {
            var option = HttpContext.RequestServices.GetRequiredService<IOptions<ApiBehaviorOptions>>();

            return (ActionResult) option.Value.InvalidModelStateResponseFactory(ControllerContext);
            
        }
    }

    
}
