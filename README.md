# Welcome to the NCEA Enricher Repository

This is the code repository for the NCEA Metadata Enricher ETL Service codebase.

# Prerequisites

Before proceeding, ensure you have the following installed:

- .NET 8 SDK: You can download and install it from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0).

# Configuration

1.  **Cloud Service Configurations**
   
    ***ServiceBus Configuration***
    We are using ServiceBusHostName to connect to ServiceBus service.
   
    Example:
    `"ServiceBusHostName": "DEVNCESBINF1401.servicebus.windows.net"`

    ***KeyVault Configuration***
    KeyVaultUri expects the KeyVault Uri to connect to the KayVault service and to get secrets like connection string, API keys, etc.
   
    Example:
    `"KeyVaultUri": "https://devnceinfst1401.blob.core.windows.net"`

    ***BlobStorage Configuration***
    BlobStorageUri config is used for connecting to Blob Storage and to access synonyms file.
   
    Example:
    `"BlobStorageUri": "https://devnceinfst1401.blob.core.windows.net"`

    ***ApplicationInsights Configuration***
    We are using ServiceBusHostName to connect to ServiceBus service.
   
    Example:
    `"ApplicationInsights": {
        "LogLevel": {
        "Default": "Information"
        }
    }`


2.  **Feature management**
 
    ***Synonym based clssification***
 
        "EnableSynonymBasedClassification": true | false
        "SynonymsContainerName"           : "ncea-classifiers"
        "SynonymsFileName"                : "Synonyms.xlsx"
        "CacheDurationInMinutes"          : 30 
    
    ***ML based clssification***  
 
        "EnableMLBasedClassification": true | false
        "ClassifierApiBaseUri"       : "https://dev-ncea-classifier.azure.defra.cloud/"         
        "CacheDurationInMinutes"     : 30
         
        Request Header : X-API-Key
        API key : Stored in Azure KeyValut Secrets (Secret Key : nceaClassifierMicroServiceApiKey)
    
    ***MDC Validations***
 
        "EnableMdcValidation": true | false