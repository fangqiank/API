using Routing.Api.Dto;
using Routing.Api.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Routing.Api.Services
{
    public class PropertyMappingService:IPropertyMappingService
    {
        private readonly Dictionary<string, PropertyMappingValue> _employeePropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                {"Id", new PropertyMappingValue(new List<string> {"Id"})},
                {"CompanyId",new PropertyMappingValue(new List<string>{"CompanyId"})},
                {"EmployeeNo",new PropertyMappingValue(new List<string>{"EmployeeNo"})},
                {"Name",new PropertyMappingValue(new List<string>{"FirstName","LastName"})},
                {"GenderDisplay",new PropertyMappingValue(new List<string>{"Gender"})},
                {"Age",new PropertyMappingValue(new List<string>{"DateOfBirth"},true )},
            };

        private readonly IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            _propertyMappings.Add(new PropertyMapping<EmployeeDto,Employee>(_employeePropertyMapping));
        }

        public Dictionary<string,PropertyMappingValue> GetPropertyMapping<TSource,TDestination>()
        {
            var matchMapping = _propertyMappings
                .OfType<PropertyMapping<TSource,TDestination>>();

            var propertyMappings = matchMapping.ToList();
            if (propertyMappings.Count() == 1)
                return propertyMappings.First().MappingDictonary;

            throw new Exception($"无法找到唯一的映射关系：{typeof(TSource)},{typeof(TDestination)}");
        }
    }
}
