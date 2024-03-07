namespace Ncea.Enricher.Infrastructure.Contracts;

public interface IKeyVaultService
{
    Task<string> GetSecretAsync(string key);
}
