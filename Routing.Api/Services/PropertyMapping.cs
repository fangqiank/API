using System;
using System.Collections.Generic;

namespace Routing.Api.Services
{
    public class PropertyMapping<TSource, TDestination>:IPropertyMapping
    {
        public Dictionary<string,PropertyMappingValue> MappingDictonary { get; private set; }

        public PropertyMapping(Dictionary<string, PropertyMappingValue> mappingDictonary)
        {
            MappingDictonary = mappingDictonary ?? throw new ArgumentNullException(nameof(mappingDictonary));
        }

    }
}
