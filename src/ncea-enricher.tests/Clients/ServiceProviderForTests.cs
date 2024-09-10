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
            .FromFile(modelName: TrainedModels.AssetTrainedModel, filePath: Path.Combine(trainedModelDirectory, "lvl1-001_Natural asset_Theme_TrainedModel.zip"), watchForChanges: false)
            .FromFile(modelName: TrainedModels.BenefitTrainedModel, filePath: Path.Combine(trainedModelDirectory, "lvl1-002_Ecosystem service or benefit_Theme_TrainedModel.zip"), watchForChanges: false)
            .FromFile(modelName: TrainedModels.PreassureTrainedModel, filePath: Path.Combine(trainedModelDirectory, "lvl1-004_Pressure_Theme_TrainedModel.zip"), watchForChanges: false)
            .FromFile(modelName: TrainedModels.ValuationTrainedModel, filePath: Path.Combine(trainedModelDirectory, "lvl1-003_Natural capital valuation_Theme_TrainedModel.zip"), watchForChanges: false);

        serviceCollection.AddPredictionEnginePool<ModelInputCategory, ModelOutput>()
            .FromFile(modelName: TrainedModels.lvl1_001, filePath: Path.Combine(trainedModelDirectory, "lvl1-001_Natural asset_Category_TrainedModel.zip"), watchForChanges: false)
            .FromFile(modelName: TrainedModels.lvl1_002, filePath: Path.Combine(trainedModelDirectory, "lvl1-002_Ecosystem service or benefit_Category_TrainedModel.zip"), watchForChanges: false)
            .FromFile(modelName: TrainedModels.lvl1_003, filePath: Path.Combine(trainedModelDirectory, "lvl1-003_Natural capital valuation_Category_TrainedModel.zip"), watchForChanges: false)
            .FromFile(modelName: TrainedModels.lvl1_004, filePath: Path.Combine(trainedModelDirectory, "lvl1-004_Pressure_Category_TrainedModel.zip"), watchForChanges: false);

        serviceCollection.AddPredictionEnginePool<ModelInputSubCategory, ModelOutput>()
            .FromFile(modelName: TrainedModels.lv2_001, filePath: Path.Combine(trainedModelDirectory, "lv2-001_Terrestrial and freshwater habitats_SubCategory_TrainedModel.zip"), watchForChanges: false)
    .FromFile(modelName: TrainedModels.lv2_002, filePath: Path.Combine(trainedModelDirectory, "lv2-002_Coastal and estuarine habitats_SubCategory_TrainedModel.zip"), watchForChanges: false)
    .FromFile(modelName: TrainedModels.lv2_003, filePath: Path.Combine(trainedModelDirectory, "lv2-003_Marine habitats_SubCategory_TrainedModel.zip"), watchForChanges: false)
    .FromFile(modelName: TrainedModels.lv2_004, filePath: Path.Combine(trainedModelDirectory, "lv2-004_Generalist species (spanning multiple habitats)_SubCategory_TrainedModel.zip"), watchForChanges: false)
    .FromFile(modelName: TrainedModels.lv2_005, filePath: Path.Combine(trainedModelDirectory, "lv2-005_Ecosystem component_SubCategory_TrainedModel.zip"), watchForChanges: false)
    .FromFile(modelName: TrainedModels.lv2_006, filePath: Path.Combine(trainedModelDirectory, "lv2-006_Provisioning services_SubCategory_TrainedModel.zip"), watchForChanges: false)
    .FromFile(modelName: TrainedModels.lv2_007, filePath: Path.Combine(trainedModelDirectory, "lv2-007_Regulating services_SubCategory_TrainedModel.zip"), watchForChanges: false)
    .FromFile(modelName: TrainedModels.lv2_008, filePath: Path.Combine(trainedModelDirectory, "lv2-008_Cultural services_SubCategory_TrainedModel.zip"), watchForChanges: false)
    .FromFile(modelName: TrainedModels.lv2_011, filePath: Path.Combine(trainedModelDirectory, "lv2-011_Climate change_SubCategory_TrainedModel.zip"), watchForChanges: false)
    .FromFile(modelName: TrainedModels.lv2_012, filePath: Path.Combine(trainedModelDirectory, "lv2-012_Chemical pollution_SubCategory_TrainedModel.zip"), watchForChanges: false)
    .FromFile(modelName: TrainedModels.lv2_013, filePath: Path.Combine(trainedModelDirectory, "lv2-013_Biological disturbances_SubCategory_TrainedModel.zip"), watchForChanges: false)
    .FromFile(modelName: TrainedModels.lv2_014, filePath: Path.Combine(trainedModelDirectory, "lv2-014_Hydrological changes_SubCategory_TrainedModel.zip"), watchForChanges: false)
    .FromFile(modelName: TrainedModels.lv2_015, filePath: Path.Combine(trainedModelDirectory, "lv2-015_Land and sea use change_SubCategory_TrainedModel.zip"), watchForChanges: false)
    .FromFile(modelName: TrainedModels.lv2_016, filePath: Path.Combine(trainedModelDirectory, "lv2-016_Other pollution or physical pressure_SubCategory_TrainedModel.zip"), watchForChanges: false);

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
