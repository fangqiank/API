using Routing.Api.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Routing.Api.Helpers;
using Routing.Api.Parameters;

namespace Routing.Api.Services
{
    public interface ICompanyRepository
    {
        Task<PagedList<Company>> GetCompaniesAsync(CompanyParameters parameters);
        Task<Company> GetCompanyAsync(Guid companyId);
        Task<IEnumerable<Company>> GetCompaniesAsync(IEnumerable<Guid> companyIds);
        void AddCompany(Company company);
        void UpdateCompany(Company company);
        void DeleteCompany(Company company);
        Task<bool> CompanyExistsAsync(Guid companyId);

        Task<IEnumerable<Employee>> GetEmployeesAsync(Guid companyId,EmployeeParameters parameters);
        Task<Employee> GetEmployeeAsync(Guid companyId, Guid employeeId);
        void AddEmployee(Guid companyId, Employee employee);
        void UpdateEmployee(Employee employee);
        void DeleteEmployee(Employee employee);

        Task<bool> SaveAsync();
    }
}
