namespace Routing.Api.Services
{
    public interface IPropertyCheckService
    {
        bool TypeHasProperties<T>(string fields);
    }
}