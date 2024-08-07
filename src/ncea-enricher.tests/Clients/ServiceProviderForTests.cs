using Microsoft.Extensions.DependencyInjection;
using Moq;
using Ncea.Enricher.Processor.Contracts;
using Microsoft.Extensions.Configuration;
using Ncea.Enricher.Processors;
using Ncea.Classifier.Microservice.Clients;
using Ncea.Enricher.Constants;
using Ncea.Enricher.Services.Contracts;
using Ncea.Enricher.Services;
using Ncea.Enricher.Models.ML;
using Microsoft.Extensions.ML;
using Newtonsoft.Json;
using Ncea.Enricher.Infrastructure.Contracts;
using Microsoft.Identity.Abstractions;

namespace Ncea.Enricher.Tests.Clients;

internal static class ServiceProviderForTests
{
    public static IServiceProvider Get()
    {
        var serviceCollection = new ServiceCollection();

        // Add any DI stuff here:


        serviceCollection.AddLogging();
        serviceCollection.AddSingleton<IEnricherService, SynonymBasedEnricher>();

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
            //.AddAzureKeyVault(new SecretClient(new Uri("https://devnceinfkvt1401.vault.azure.net/"), new DefaultAzureCredential()), new KeyVaultSecretManager())
           .Build();
        serviceCollection.AddSingleton<IConfiguration>(configuration);

        var classifierVocabularyFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "ClassifierVocabulary.json");
        var classifierVocabulary = JsonConvert.DeserializeObject<List<ClassifierInfo>>(File.ReadAllText(classifierVocabularyFilePath));

        //var classifierMicroserviceClientMock = new Mock<INceaClassifierMicroserviceClient>();
        //classifierMicroserviceClientMock.Setup(s => s.VocabularyAsync(It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(classifierVocabulary);

        var downstreamApiMock = new Mock<IDownstreamApi>();
        downstreamApiMock.Setup(s => s.GetForAppAsync<ICollection<ClassifierInfo>>("ClassifierApi", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(classifierVocabulary);

        serviceCollection.AddSingleton<IBlobService>(BlobServiceForTests.Get());

        serviceCollection.AddSingleton(downstreamApiMock.Object);

        serviceCollection.AddSingleton<IClassifierPredictionService, ClassifierPredictionService>();

        var trainedModelDirectory = "MLTrainedModels";
        serviceCollection.AddPredictionEnginePool<ModelInputTheme, ModelOutput>()
            .FromFile(modelName: TrainedModels.Asset, filePath: Path.Combine(trainedModelDirectory, "AssetTrainedModel.zip"), watchForChanges: false)
            .FromFile(modelName: TrainedModels.Preassure, filePath: Path.Combine(trainedModelDirectory, "PressureTrainedModel.zip"), watchForChanges: false)
            .FromFile(modelName: TrainedModels.Benefit, filePath: Path.Combine(trainedModelDirectory, "BenefitTrainedModel.zip"), watchForChanges: false)
            .FromFile(modelName: TrainedModels.Valuation, filePath: Path.Combine(trainedModelDirectory, "ValuationTrainedModel.zip"), watchForChanges: false)
            .FromFile(modelName: TrainedModels.Theme, filePath: Path.Combine(trainedModelDirectory, "ThemeTrainedModel.zip"), watchForChanges: false);

        serviceCollection.AddPredictionEnginePool<ModelInputCategory, ModelOutput>()
            .FromFile(modelName: TrainedModels.Category, filePath: Path.Combine(trainedModelDirectory, "CategoryTrainedModel.zip"), watchForChanges: false);

        serviceCollection.AddPredictionEnginePool<ModelInputSubCategory, ModelOutput>()
            .FromFile(modelName: TrainedModels.SubCategory, filePath: Path.Combine(trainedModelDirectory, "SubCategoryTrainedModel.zip"), watchForChanges: false);

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
