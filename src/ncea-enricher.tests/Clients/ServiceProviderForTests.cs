using Microsoft.Extensions.DependencyInjection;
using Moq;
using Ncea.Enricher.Processor.Contracts;
using Microsoft.Extensions.Configuration;
using Ncea.Enricher.Processors;
using Ncea.Classifier.Microservice;
using Ncea.Enricher.Constants;
using Ncea.Enricher.Services.Contracts;
using Ncea.Enricher.Services;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Ncea.Enricher.Models.ML;
using Microsoft.Extensions.ML;

namespace Ncea.Enricher.Tests.Clients;

internal static class ServiceProviderForTests
{
    public static IServiceProvider Get()
    {
        var serviceCollection = new ServiceCollection();

        // Add any DI stuff here:


        serviceCollection.AddLogging();
        serviceCollection.AddSingleton<IEnricherService, MdcEnricher>();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(
                 path: "appsettings.json",
                 optional: false,
                 reloadOnChange: true)
            .AddJsonFile(
                 path: "appsettings-fieldconfigurations.json",
                 optional: false,
                 reloadOnChange: true)
            .AddAzureKeyVault(new SecretClient(new Uri("https://devnceinfkvt1401.vault.azure.net/"), new DefaultAzureCredential()), new KeyVaultSecretManager())
           .Build();
        serviceCollection.AddSingleton<IConfiguration>(configuration);

        serviceCollection.AddHttpClient<INceaClassifierMicroserviceClient, NceaClassifierMicroserviceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetValue<string>("ClassifierApiUri")!);
            client.DefaultRequestHeaders.Add(ApiKeyParameters.ApiKeyHeaderName, configuration.GetValue<string>(ApiKeyParameters.ApiKeyName)!);
        });

        serviceCollection.AddSingleton<IClassifierPredictionService, ClassifierPredictionService>();

        serviceCollection.AddPredictionEnginePool<ModelInputTheme, ModelOutput>()
            .FromFile(modelName: TrainedModels.Theme, filePath: Path.Combine("MLTrainedModels", "ThemeTrainedModel.zip"), watchForChanges: false);

        serviceCollection.AddPredictionEnginePool<ModelInputCategory, ModelOutput>()
            .FromFile(modelName: TrainedModels.Category, filePath: Path.Combine("MLTrainedModels", "ThemeTrainedModel.zip"), watchForChanges: false);

        serviceCollection.AddPredictionEnginePool<ModelInputSubCategory, ModelOutput>()
            .FromFile(modelName: TrainedModels.Subcategory, filePath: Path.Combine("MLTrainedModels", "ThemeTrainedModel.zip"), watchForChanges: false);

        serviceCollection.AddMemoryCache();

        serviceCollection.AddSingleton<ISynonymsProvider, SynonymsProvider>();
        serviceCollection.Decorate<ISynonymsProvider, CachedSynonymsProvider>();

        serviceCollection.AddSingleton<IClassifierVocabularyProvider, ClassifierVocabularyProvider>();
        serviceCollection.Decorate<IClassifierVocabularyProvider, CachedClassifierVocabularyProvider>();
        serviceCollection.AddLogging();

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
