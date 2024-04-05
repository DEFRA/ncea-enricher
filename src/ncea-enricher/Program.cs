using Azure.Identity;
using Ncea.Enricher;
using Ncea.Enricher.Infrastructure;
using Azure.Messaging.ServiceBus;
using Ncea.Enricher.Infrastructure.Contracts;
using Ncea.Enricher.Processors.Contracts;
using Azure.Security.KeyVault.Secrets;
using Azure.Messaging.ServiceBus.Administration;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.Extensions.Azure;
using ncea.enricher.Processor;
using ncea.enricher.Processor.Contracts;
using Ncea.Enricher.Processors;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Storage.Files.Shares;
using Ncea.Enricher.Constants;
using Azure.Storage.Blobs;
using Ncea.Enricher.Processor;

var configuration = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json")
                                .AddEnvironmentVariables()
                                .Build();

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddHealthChecks().AddCheck<HealthCheck>("custom_hc");
builder.Services.AddHostedService<TcpHealthProbeService>();

builder.Services.AddHttpClient();

ConfigureKeyVault(configuration, builder);
ConfigureBlobStorage(configuration, builder);
ConfigureLogging(builder);
await ConfigureServiceBusQueue(configuration, builder);
ConfigureFileShareClient(configuration, builder);
ConfigureServices(builder);

var host = builder.Build();
host.Run();

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

static void ConfigureFileShareClient(IConfigurationRoot configuration, HostApplicationBuilder builder)
{    
    var fileSharePath = configuration.GetValue<string>("FileShareName");
    foreach (string dataSourceName in Enum.GetNames(typeof(DataSourceNames)))
    {
        var dirPath = Path.Combine(fileSharePath!, dataSourceName);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
    }

    var fileShareConnectionString = builder.Configuration.GetValue<string>("FileShare:ConnectionString");
    builder.Services.AddAzureClients(builder =>
    {
        builder.AddFileServiceClient(fileShareConnectionString);
        builder.UseCredential(new DefaultAzureCredential());

        builder.AddClient<ShareClient, ShareClientOptions>(
            (_, _, provider) => provider.GetService<ShareServiceClient>()!.GetShareClient(fileSharePath))
        .WithName(fileSharePath);
    });
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
    builder.Services.AddApplicationInsightsTelemetryWorkerService();
    builder.Services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
    {
        module.EnableSqlCommandTextInstrumentation = true;
        o.ConnectionString = builder.Configuration.GetValue<string>("ApplicationInsights:ConnectionString");
    });
}

static void ConfigureServices(HostApplicationBuilder builder)
{
    builder.Services.AddSingleton<IApiClient, ApiClient>();
    builder.Services.AddSingleton<IOrchestrationService, OrchestrationService>();
    builder.Services.AddSingleton<IKeyVaultService, KeyVaultService>();

    builder.Services.AddMemoryCache();
    builder.Services.AddScoped<ISynonymsProvider, SynonymsProvider>();
    builder.Services.Decorate<ISynonymsProvider, CachedSynonymsProvider>();

    builder.Services.AddKeyedSingleton<IEnricherService, JnccEnricher>("Jncc");
    builder.Services.AddKeyedSingleton<IEnricherService, MedinEnricher>("Medin");
}

static async Task CreateServiceBusQueueIfNotExist(ServiceBusAdministrationClient servicebusAdminClient, string queueName)
{    
    bool queueExists = await servicebusAdminClient.QueueExistsAsync(queueName);
    if (!queueExists)
    {
        await servicebusAdminClient.CreateQueueAsync(queueName);
    }
}

[ExcludeFromCodeCoverage]
public static partial class Program { }