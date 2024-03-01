namespace Ncea.Enricher.Infrastructure.Contracts;

public interface IApiClient
{
    void CreateClient(string BaseUrl);
    Task<string> GetAsync(string apiUrl);
    Task<string> PostAsync(string apiUrl, string requestData);
}
