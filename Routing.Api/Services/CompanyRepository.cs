using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Routing.Api.Data;
using Routing.Api.Dto;
using Routing.Api.Entities;
using Routing.Api.Helpers;
using Routing.Api.Parameters;

namespace Routing.Api.Services
{
    public class CompanyRepository:ICompanyRepository
    {
        private readonly RoutingDbContext _context;
        private readonly IPropertyMappingService _propertyMappingService;

        public CompanyRepository(RoutingDbContext context,IPropertyMappingService propertyMappingService)
        {
            this._context = context ?? throw  new ArgumentNullException(nameof(context));
            this._propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
        }

        //public async Task<IEnumerable<Company>> GetCompaniesAsync(CompanyParameters parameters)

        //PagedList class 分页类
        public async Task<PagedList<Company>> GetCompaniesAsync(CompanyParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            //if (string.IsNullOrWhiteSpace(parameters.CompanyName) &&
            //    string.IsNullOrWhiteSpace(parameters.SearchTerm))
            //{
            //    return await _context.Companies.ToListAsync();
            //}

            var queryExpression = _context.Companies as IQueryable<Company>;
            //IQueryable：使用EFCore动态拼接多个where条件时使用.ToListAsync(),执行数据库 (延迟查询，每次真正使用时都会重新读取数据。)

            if (!string.IsNullOrWhiteSpace(parameters.CompanyName))
            {
                parameters.CompanyName = parameters.CompanyName.Trim();
                queryExpression = queryExpression.Where(x => x.Name == parameters.CompanyName);
            }

            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                parameters.SearchTerm = parameters.SearchTerm.Trim();
                queryExpression = queryExpression.Where(x => x.Name.Contains(parameters.SearchTerm) ||
                                                             x.Introduction.Contains(parameters.SearchTerm));
            }

            //分页在过滤，搜索之后
            //queryExpression=queryExpression.Skip(parameters.PageSize * (parameters.PageNumber - 1))
            //    .Take(parameters.PageSize);

            
            //return await queryExpression.ToListAsync();
            var mappingDictonary =
                _propertyMappingService.GetPropertyMapping<CompanyDto, Company>();

            queryExpression = queryExpression.ApplySort(parameters.OrderBy, mappingDictonary);
            
            return await PagedList<Company>.CreateAsync(queryExpression,parameters.PageNumber,parameters.PageSize);

        }

        public async Task<Company> GetCompanyAsync(Guid companyId)
        {
            if (companyId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(companyId));
            }

            return await _context.Companies
                .FirstOrDefaultAsync(x => x.Id == companyId);
        }

        public async Task<IEnumerable<Company>> GetCompaniesAsync(IEnumerable<Guid> companyIds)
        {
            if (companyIds == null)
            {
                throw new ArgumentNullException(nameof(companyIds));
            }

            return await _context.Companies
                .Where(x => companyIds.Contains(x.Id))
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public void AddCompany(Company company)
        {
            if (company == null)
            {
                throw new ArgumentNullException(nameof(company));
            }

            company.Id = Guid.NewGuid();


            if (company.Employees != null)
            {
                foreach (var employee in company.Employees)
                {
                    employee.Id = Guid.NewGuid();
                }
            }
            
            _context.Companies.Add(company);
        }

        public void UpdateCompany(Company company)
        {
           //_context.Entry(company).State = EntityState.Modified;
        }

        public void DeleteCompany(Company company)
        {
            if (company == null)
            {
                throw new ArgumentNullException(nameof(company));
            }

            _context.Companies.Remove(company);
        }

        public async Task<bool> CompanyExistsAsync(Guid companyId)
        {
            if (companyId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(companyId));
            }

            return await _context.Companies.AnyAsync(x => x.Id == companyId);
        }

        //public async Task<IEnumerable<Employee>> GetEmployeesAsync(Guid companyId,string genderDisplay,string q)
        public async Task<IEnumerable<Employee>> GetEmployeesAsync(Guid companyId, EmployeeParameters parameters)
        {
            if (companyId == Guid.Empty)
            {
                throw  new ArgumentNullException();
            }

            //过滤(filter by gender),搜索 q
            //if (string.IsNullOrWhiteSpace(parameters.Gender) && string.IsNullOrWhiteSpace(parameters.Q))
            //{
            //    return await _context.Employees
            //        .Where(x => x.CompanyId == companyId)
            //        .OrderBy(x => x.EmployeeNo)
            //        .ToListAsync();
            //}


            var items = _context.Employees.Where(x=>x.CompanyId==companyId);

            if (!string.IsNullOrWhiteSpace(parameters.Gender))
            {
                parameters.Gender = parameters.Gender.Trim();

                var gender = Enum.Parse<Gender>(parameters.Gender);
                
                items = items.Where(x => x.Gender == gender);
            }

            if (!string.IsNullOrWhiteSpace(parameters.Q))
            {
                parameters.Q = parameters.Q.Trim();
                items = items.Where(
                    x => x.EmployeeNo.Contains(parameters.Q) 
                         || x.FirstName.Contains(parameters.Q) 
                         || x.LastName.Contains(parameters.Q)
                );
            }

            //if (!string.IsNullOrWhiteSpace(parameters.OrderBy))
            //{
            //    if (parameters.OrderBy.ToLowerInvariant() == "name") 
                    //如果您的应用程序依赖于字符串以可预测的方式更改而不受当前区域性影响的情况,
                    //请使用ToLowerInvariant方法.ToLowerInvariant方法
                    //等同于ToLower(CultureInfo.InvariantCulture).
                    //当字符串集合必须以可预测的顺序出现在用户界面控件中时,建议使用此方法
            //        items = items.OrderBy(x => x.FirstName)
            //                     .ThenBy(x => x.LastName);
            //}

            var mappingDictionary =
                _propertyMappingService.GetPropertyMapping<EmployeeDto, Employee>();

            items=items.ApplySort(parameters.OrderBy, mappingDictionary);

            return await items
                //.OrderBy(x => x.EmployeeNo)
                .ToListAsync();
        }

        public async Task<Employee> GetEmployeeAsync(Guid companyId, Guid employeeId)
        {
            if (companyId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(companyId));
            }

            if (employeeId == Guid.Empty)
            {
                throw new ArgumentNullException();
            }

            return await _context.Employees
                .Where(x => x.CompanyId == companyId && x.Id == employeeId)
                .FirstOrDefaultAsync();
        }

        public void AddEmployee(Guid companyId, Employee employee)
        {
            if (companyId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(companyId));
            }

            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            employee.CompanyId = companyId;
            _context.Employees.Add(employee);
        }

        public void UpdateEmployee(Employee employee)  //无代码，Dbcontext entity framework core处理
        {
            //_context.Entry(employee).State = EntityState.Modified;
        }

        public void DeleteEmployee(Employee employee)
        {
            _context.Employees.Remove(employee);
        }

        public async Task<bool> SaveAsync()
        {
            return await _context.SaveChangesAsync() >= 0;
        }
    }
}
