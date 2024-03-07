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
using Azure.Storage.Blobs;
using Ncea.Enricher.Constants;

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
ConfigureLogging(builder);
await ConfigureServiceBusQueue(configuration, builder);
await ConfigureFileShareClient(configuration, builder);
await ConfigureBlobStorage(configuration, builder);
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

static async Task ConfigureFileShareClient(IConfigurationRoot configuration, HostApplicationBuilder builder)
{    
    var fileShareName = configuration.GetValue<string>("FileShareName");

    var fileShareConnectionString = builder.Configuration.GetValue<string>("FileShare:ConnectionString");
    await InitializeFileShare(fileShareName, fileShareConnectionString);

    builder.Services.AddAzureClients(builder =>
    {
        builder.AddFileServiceClient(fileShareConnectionString);
        builder.UseCredential(new DefaultAzureCredential());

        builder.AddClient<ShareClient, ShareClientOptions>(
            (_, _, provider) => provider.GetService<ShareServiceClient>()!.GetShareClient(fileShareName))
        .WithName(fileShareName);
    });
}

static void ConfigureKeyVault(IConfigurationRoot configuration, HostApplicationBuilder builder)
{
    var keyVaultEndpoint = new Uri(configuration.GetValue<string>("KeyVaultUri")!);
    var secretClient = new SecretClient(keyVaultEndpoint, new DefaultAzureCredential());
    builder.Configuration.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
    builder.Services.AddSingleton(secretClient);
}

static async Task ConfigureBlobStorage(IConfigurationRoot configuration, HostApplicationBuilder builder)
{
    var blobStorageEndpoint = new Uri(configuration.GetValue<string>("BlobStorageUri")!);
    var blobServiceClient = new BlobServiceClient(blobStorageEndpoint, new DefaultAzureCredential());

    builder.Services.AddSingleton(x => blobServiceClient);

    var blobContainerName = configuration.GetValue<string>("FileShareName");

    BlobContainerClient container = blobServiceClient.GetBlobContainerClient(blobContainerName);
    await container.CreateIfNotExistsAsync();
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
    builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();
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

static async Task InitializeFileShare(string? fileShareName, string? fileShareConnectionString)
{
    var shareClient = new ShareClient(fileShareConnectionString, fileShareName);
    await shareClient.CreateIfNotExistsAsync();
    foreach (string dataSourceName in Enum.GetNames(typeof(DataSourceNames)))
    {
        var directory = shareClient.GetDirectoryClient(dataSourceName);
        await directory.CreateIfNotExistsAsync();
    }
}

[ExcludeFromCodeCoverage]
public static partial class Program { }