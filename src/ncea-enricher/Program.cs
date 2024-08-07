using Azure.Identity;
using Ncea.Enricher;
using Ncea.Enricher.Infrastructure;
using Azure.Messaging.ServiceBus;
using Ncea.Enricher.Infrastructure.Contracts;
using Azure.Security.KeyVault.Secrets;
using Azure.Messaging.ServiceBus.Administration;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.Extensions.Azure;
using Ncea.Enricher.Processors;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Storage.Blobs;
using Ncea.Enricher.Processor.Contracts;
using Ncea.Enricher.Services.Contracts;
using Ncea.Enricher.Services;
using Microsoft.FeatureManagement;
using Ncea.Enricher.Enums;
using Microsoft.Extensions.ML;
using Ncea.Enricher.Models.ML;
using Ncea.Enricher.Constants;
using Microsoft.Identity.Web;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;

var configuration = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json")
                                .AddJsonFile("appsettings-fieldconfigurations.json")
                                .AddEnvironmentVariables()
                                .Build();

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddHealthChecks().AddCheck<HealthCheck>("custom_hc");
builder.Services.AddHostedService<TcpHealthProbeService>();

builder.Services.AddFeatureManagement(configuration.GetSection("FeatureManagement"));

ConfigureKeyVault(configuration, builder);
ConfigureBlobStorage(configuration, builder);
ConfigureLogging(builder);
await ConfigureServiceBusQueue(configuration, builder);
ConfigureFileShareClient(configuration);
ConfigureClassifierApi(builder);
ConfigureServices(builder);
ConfigureMachineLearningModels(builder);

var host = builder.Build();
await host.RunAsync();

static async Task ConfigureServiceBusQueue(IConfigurationRoot configuration, HostApplicationBuilder builder)
{
    var mapperQueueName = configuration.GetValue<string>("MapperQueueName");
    var servicebusHostName = configuration.GetValue<string>("ServiceBusHostName");

    var createQueue = configuration.GetValue<bool>("DynamicQueueCreation");
    if (createQueue)
    {
        var servicebusAdminClient = new ServiceBusAdministrationClient(servicebusHostName, new DefaultAzureCredential());
        await CreateServiceBusQueueIfNotExist(servicebusAdminClient, mapperQueueName!);
    }      

    builder.Services.AddAzureClients(builder =>
    {
        builder.AddServiceBusClientWithNamespace(servicebusHostName);
        builder.UseCredential(new DefaultAzureCredential());

        builder.AddClient<ServiceBusProcessor, ServiceBusClientOptions>(
            (_, _, provider) => provider.GetService<ServiceBusClient>()!.CreateProcessor(mapperQueueName))
            .WithName(mapperQueueName);
    });   
}

static void ConfigureFileShareClient(IConfigurationRoot configuration)
{    
    var fileSharePath = configuration.GetValue<string>("FileShareName");
    foreach (string dataSourceName in Enum.GetNames(typeof(DataSource)))
    {
        var dirPath = Path.Combine(fileSharePath!, dataSourceName.ToLowerInvariant());
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
    }
}

static void ConfigureKeyVault(IConfigurationRoot configuration, HostApplicationBuilder builder)
{
    var keyVaultEndpoint = new Uri(configuration.GetValue<string>("KeyVaultUri")!);
    var secretClient = new SecretClient(keyVaultEndpoint, new DefaultAzureCredential());
    builder.Configuration.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
    builder.Services.AddSingleton(secretClient);
}

static void ConfigureBlobStorage(IConfigurationRoot configuration, HostApplicationBuilder builder)
{
    var blobStorageEndpoint = new Uri(configuration.GetValue<string>("BlobStorageUri")!);
    var blobServiceClient = new BlobServiceClient(blobStorageEndpoint, new DefaultAzureCredential());

    builder.Services.AddSingleton(x => blobServiceClient);
}

static void ConfigureLogging(HostApplicationBuilder builder)
{
    builder.Services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddApplicationInsights(
            configureTelemetryConfiguration: (config) =>
                config.ConnectionString = builder.Configuration.GetValue<string>("ApplicationInsights:ConnectionString"),
                configureApplicationInsightsLoggerOptions: (options) => { }
            );
        loggingBuilder.AddConsole();
        loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>(null, LogLevel.Information);

    });
    builder.Services.AddApplicationInsightsTelemetryWorkerService(o => o.EnableAdaptiveSampling = false);
    builder.Services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
    {
        module.EnableSqlCommandTextInstrumentation = true;
        o.ConnectionString = builder.Configuration.GetValue<string>("ApplicationInsights:ConnectionString");
    });
}

static void ConfigureClassifierApi(HostApplicationBuilder builder)
{
    var clientId = builder.Configuration.GetValue<string>("daemon-app-clientid")!;
    var classifierApiClientId = builder.Configuration.GetValue<string>("classifier-app-api-clientid")!;

    var azureAdSection = builder.Configuration.GetSection("AzureAd");
    
    azureAdSection.GetSection("ClientId").Value = clientId;
    azureAdSection.GetSection("ClientCredentials:0:ClientSecret").Value = builder.Configuration.GetValue<string>("daemon-app-secret");

    builder.Services.AddTokenAcquisition(isTokenAcquisitionSingleton: true)
        .Configure<MicrosoftIdentityApplicationOptions>(builder.Configuration.GetSection("AzureAd"))
        .AddInMemoryTokenCaches()
        .AddHttpClient();

    var classifierApiSection = builder.Configuration.GetSection("ClassifierApi");
    var apiScope = $"api://{classifierApiClientId}/.default";
    classifierApiSection.GetSection("Scopes:0").Value = apiScope;
    builder.Services.AddDownstreamApi("ClassifierApi", builder.Configuration.GetSection("ClassifierApi"));
}

static void ConfigureServices(HostApplicationBuilder builder)
{
    builder.Services.AddSingleton<IApiClient, ApiClient>();
    builder.Services.AddSingleton<IOrchestrationService, OrchestrationService>();
    builder.Services.AddSingleton<IBlobService, BlobService>();
    builder.Services.AddSingleton<IMdcFieldConfigurationService, MdcFieldConfigurationService>();
    builder.Services.AddSingleton<ISearchService, SearchService>();
    builder.Services.AddSingleton<IXmlNodeService, XmlNodeService>();
    builder.Services.AddSingleton<IXmlValidationService, XPathValidationService>();
    builder.Services.AddSingleton<IClassifierPredictionService, ClassifierPredictionService>();

    builder.Services.AddMemoryCache();

    builder.Services.AddSingleton<ISynonymsProvider, SynonymsProvider>();
    builder.Services.Decorate<ISynonymsProvider, CachedSynonymsProvider>();

    builder.Services.AddSingleton<IClassifierVocabularyProvider, ClassifierVocabularyProvider>();
    builder.Services.Decorate<IClassifierVocabularyProvider, CachedClassifierVocabularyProvider>();

    var isSynonymBasedClassificationEnabled = builder.Configuration.GetValue<bool>("FeatureManagement:EnableSynonymBasedClassification");
    if (isSynonymBasedClassificationEnabled)
    {
        builder.Services.AddSingleton<IEnricherService, SynonymBasedEnricher>();
    }
    else
    {
        builder.Services.AddSingleton<IEnricherService, MLBasedEnricher>();
    }
}

static async Task CreateServiceBusQueueIfNotExist(ServiceBusAdministrationClient servicebusAdminClient, string queueName)
{    
    bool queueExists = await servicebusAdminClient.QueueExistsAsync(queueName);
    if (!queueExists)
    {
        await servicebusAdminClient.CreateQueueAsync(queueName);
    }
}

static void ConfigureMachineLearningModels(HostApplicationBuilder builder)
{
    var trainedModelDirectory = "MLTrainedModels";
    builder.Services.AddPredictionEnginePool<ModelInputTheme, ModelOutput>()
    .FromFile(modelName: TrainedModels.Asset, filePath: Path.Combine(trainedModelDirectory, "AssetTrainedModel.zip"), watchForChanges: false)
    .FromFile(modelName: TrainedModels.Preassure, filePath: Path.Combine(trainedModelDirectory, "PressureTrainedModel.zip"), watchForChanges: false)
    .FromFile(modelName: TrainedModels.Benefit, filePath: Path.Combine(trainedModelDirectory, "BenefitTrainedModel.zip"), watchForChanges: false)
    .FromFile(modelName: TrainedModels.Valuation, filePath: Path.Combine(trainedModelDirectory, "ValuationTrainedModel.zip"), watchForChanges: false)
    .FromFile(modelName: TrainedModels.Theme, filePath: Path.Combine(trainedModelDirectory, "ThemeTrainedModel.zip"), watchForChanges: false);

    builder.Services.AddPredictionEnginePool<ModelInputCategory, ModelOutput>()
    .FromFile(modelName: TrainedModels.Category, filePath: Path.Combine(trainedModelDirectory, "CategoryTrainedModel.zip"), watchForChanges: false);

    builder.Services.AddPredictionEnginePool<ModelInputSubCategory, ModelOutput>()
    .FromFile(modelName: TrainedModels.SubCategory, filePath: Path.Combine(trainedModelDirectory, "SubCategoryTrainedModel.zip"), watchForChanges: false);
}

[ExcludeFromCodeCoverage]
public static partial class Program { }