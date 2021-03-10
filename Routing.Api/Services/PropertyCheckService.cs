using System.Reflection;

namespace Routing.Api.Services
{
    public class PropertyCheckService : IPropertyCheckService
    {
        public bool TypeHasProperties<T>(string fields)
        {
            if (string.IsNullOrWhiteSpace(fields))
                return true;

            var fieldsAfterSplit = fields.Split(",");

            foreach (var field in fieldsAfterSplit)
            {
                var propertyName = field.Trim();
                var property = typeof(T).GetProperty(propertyName,
                    BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);

                if (property == null)
                    return false;
            }

            return true;

        }
    }
}
