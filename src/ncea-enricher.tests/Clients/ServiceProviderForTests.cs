using Microsoft.Extensions.DependencyInjection;
using Moq;
using Ncea.Enricher.Processors;
using Ncea.Enricher.Processor.Contracts;

namespace Ncea.Enricher.Tests.Clients;

internal static class ServiceProviderForTests
{
    public static IServiceProvider Get()
    {
        var serviceCollection = new ServiceCollection();

        // Add any DI stuff here:
        serviceCollection.AddLogging();
        serviceCollection.AddKeyedSingleton<IEnricherService, JnccEnricher>("Jncc");
        serviceCollection.AddKeyedSingleton<IEnricherService, MedinEnricher>("Medin");        
        
        // Create the ServiceProvider
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // serviceScopeMock will contain ServiceProvider
        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.SetupGet<IServiceProvider>(s => s.ServiceProvider)
            .Returns(serviceProvider);

        // serviceScopeFactoryMock will contain serviceScopeMock
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock.Setup(s => s.CreateScope())
            .Returns(serviceScopeMock.Object);
        var mockServiceProvider = serviceScopeFactoryMock.Object.CreateScope().ServiceProvider;
        return mockServiceProvider;
    }
}
