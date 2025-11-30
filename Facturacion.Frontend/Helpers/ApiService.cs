using Facturacion.Shared.Responses;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Helpers;

public class ApiService : IApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiBaseUrl;
    private JsonSerializerOptions _jsonDefaultOptions => new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    };

    public ApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _apiBaseUrl = configuration["ApiBaseUrl"]!;
    }

    public async Task<ActionResponse<T>> DeleteAsync<T>(string url)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var responseHttp = await httpClient.DeleteAsync($"{_apiBaseUrl}{url}");

            if (responseHttp.IsSuccessStatusCode)
            {
                return new ActionResponse<T>
                {
                    WasSuccess = true
                };
            }

            return new ActionResponse<T>
            {
                WasSuccess = false,
                Message = await responseHttp.Content.ReadAsStringAsync()
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<T>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }

    public async Task<ActionResponse<T>> GetAsync<T>(string url)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var responseHttp = await httpClient.GetAsync($"{_apiBaseUrl}{url}");

            if (responseHttp.IsSuccessStatusCode)
            {
                var response = await responseHttp.Content.ReadFromJsonAsync<T>(_jsonDefaultOptions);
                return new ActionResponse<T>
                {
                    WasSuccess = true,
                    Result = response
                };
            }

            return new ActionResponse<T>
            {
                WasSuccess = false,
                Message = await responseHttp.Content.ReadAsStringAsync()
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<T>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }

    public async Task<ActionResponse<T>> PostAsync<T>(string url, T model)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var messageJSON = JsonSerializer.Serialize(model);
            var messageContent = new StringContent(messageJSON, Encoding.UTF8, "application/json");
            var responseHttp = await httpClient.PostAsync($"{_apiBaseUrl}{url}", messageContent);

            if (responseHttp.IsSuccessStatusCode)
            {
                var response = await responseHttp.Content.ReadFromJsonAsync<T>(_jsonDefaultOptions);
                return new ActionResponse<T>
                {
                    WasSuccess = true,
                    Result = response
                };
            }

            return new ActionResponse<T>
            {
                WasSuccess = false,
                Message = await responseHttp.Content.ReadAsStringAsync()
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<T>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }

    public async Task<ActionResponse<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest model)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var messageJSON = JsonSerializer.Serialize(model);
            var messageContent = new StringContent(messageJSON, Encoding.UTF8, "application/json");
            var responseHttp = await httpClient.PostAsync($"{_apiBaseUrl}{url}", messageContent);

            if (responseHttp.IsSuccessStatusCode)
            {
                var response = await responseHttp.Content.ReadFromJsonAsync<TResponse>(_jsonDefaultOptions);
                return new ActionResponse<TResponse>
                {
                    WasSuccess = true,
                    Result = response
                };
            }

            return new ActionResponse<TResponse>
            {
                WasSuccess = false,
                Message = await responseHttp.Content.ReadAsStringAsync()
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<TResponse>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }

    public async Task<ActionResponse<T>> PutAsync<T>(string url, T model)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var messageJSON = JsonSerializer.Serialize(model);
            var messageContent = new StringContent(messageJSON, Encoding.UTF8, "application/json");
            var responseHttp = await httpClient.PutAsync($"{_apiBaseUrl}{url}", messageContent);

            if (responseHttp.IsSuccessStatusCode)
            {
                var response = await responseHttp.Content.ReadFromJsonAsync<T>(_jsonDefaultOptions);
                return new ActionResponse<T>
                {
                    WasSuccess = true,
                    Result = response
                };
            }

            return new ActionResponse<T>
            {
                WasSuccess = false,
                Message = await responseHttp.Content.ReadAsStringAsync()
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<T>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }

    public async Task<ActionResponse<TResponse>> PutAsync<TRequest, TResponse>(string url, TRequest model)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var messageJSON = JsonSerializer.Serialize(model);
            var messageContent = new StringContent(messageJSON, Encoding.UTF8, "application/json");
            var responseHttp = await httpClient.PutAsync($"{_apiBaseUrl}{url}", messageContent);

            if (responseHttp.IsSuccessStatusCode)
            {
                var response = await responseHttp.Content.ReadFromJsonAsync<TResponse>(_jsonDefaultOptions);
                return new ActionResponse<TResponse>
                {
                    WasSuccess = true,
                    Result = response
                };
            }

            return new ActionResponse<TResponse>
            {
                WasSuccess = false,
                Message = await responseHttp.Content.ReadAsStringAsync()
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<TResponse>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }
}
