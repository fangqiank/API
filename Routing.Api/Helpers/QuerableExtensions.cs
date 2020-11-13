using Routing.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Routing.Api.Helpers
{
    public static class QuerableExtensions
    {
        public static IQueryable<T> ApplySort<T>(
            this IQueryable<T> source,
            string orderBy, 
            Dictionary<string, PropertyMappingValue> mappingValues)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if(mappingValues==null)
                throw new ArgumentNullException(nameof(mappingValues));

            if (string.IsNullOrWhiteSpace(orderBy))
                return source;

            var orderByAfterSplit = orderBy.Split(",");

            foreach (var clause in orderByAfterSplit.Reverse())
            {
                var trimmedClause = clause.Trim();

                var orderDescending = trimmedClause.EndsWith(" desc");
                Console.WriteLine(orderDescending);

                var indexOfFirstSpace = trimmedClause.IndexOf(" ", StringComparison.Ordinal);

                var propertyName = indexOfFirstSpace == -1
                    ? trimmedClause
                    : trimmedClause.Remove(indexOfFirstSpace);

                

                if (!mappingValues.ContainsKey(propertyName))
                    throw new ArgumentException($"没有找到key为{propertyName}的映射");

                var propertyMappingValue = mappingValues[propertyName];

                if (propertyMappingValue==null)
                    throw new ArgumentNullException(nameof(propertyMappingValue));

                foreach (var dest in propertyMappingValue.DestinationProperties.Reverse())
                {
                    

                    if (propertyMappingValue.Revert)
                    {
                        orderDescending = !orderDescending;
                    }
                    Console.WriteLine($"dest:{dest}");
                    source = source.OrderBy(dest +
                                            (orderDescending ? " descending" : " ascending"));
                }
            }

            return source;
        }

    }
}
