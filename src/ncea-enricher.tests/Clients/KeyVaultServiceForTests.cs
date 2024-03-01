using Azure.Security.KeyVault.Secrets;
using Azure;
using Moq;
using Ncea.Enricher.Infrastructure;

namespace Ncea.Enricher.Tests.Clients;

public static class KeyVaultServiceForTests
{
    public static KeyVaultService Get(string key, string value)
    {
        var mockSecretClient = new Mock<SecretClient>();
        var mockSecret = new KeyVaultSecret(key, value);
        var mockResponse = Response.FromValue(mockSecret, new Mock<Response>().Object);
        mockSecretClient.Setup<Task<Azure.Response<KeyVaultSecret>>>(x => x.GetSecretAsync(It.IsAny<string>(), null, default))
                        .ReturnsAsync(mockResponse);
        var secretClient = mockSecretClient.Object;
        var clientService = new KeyVaultService(secretClient);

        return clientService;
    }
}
