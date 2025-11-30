using Facturacion.Shared.Responses;

namespace Facturacion.Frontend.Helpers;

public interface IApiService
{
    Task<ActionResponse<T>> GetAsync<T>(string url);
    Task<ActionResponse<T>> PostAsync<T>(string url, T model);
    Task<ActionResponse<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest model);
    Task<ActionResponse<T>> PutAsync<T>(string url, T model);
    Task<ActionResponse<TResponse>> PutAsync<TRequest, TResponse>(string url, TRequest model);
    Task<ActionResponse<T>> DeleteAsync<T>(string url);
}
