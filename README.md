# Welcome to the NCEA Enricher ETL Service Repository

This is the code repository for the NCEA Metadata Enricher ETL Service codebase.

# Prerequisites

Before proceeding, ensure you have the following installed:

- .NET 8 SDK: You can download and install it from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0).


# Configurations

## Azure Dependencies
   
***ServiceBus Configurations:***
    *ServiceBusHostName* to connect to ServiceBus, to send messages in servicebus queues and to dynamically create queues, if the *DynamicQueueCreation* is set to *True*   

    "ServiceBusHostName": "DEVNCESBINF1401.servicebus.windows.net"
    "HarvesterQueueName": "harvested-queue"
    "MapperQueueName": "mapped-queue",
    "DynamicQueueCreation": true,

***KeyVault Configurations:***
    *KeyVaultUri* to access Azure KeyVault and to access secrets and connection strings.   

    "KeyVaultUri": "https://devnceinfkvt1401.vault.azure.net/"

***BlobStorage Configuration:***
    *BlobStorageUri* to connect to Azure Blob Storage, to create containers per DataSource and to Save the XML files for the respective data source.
       
    "BlobStorageUri": "https://devnceinfst1401.blob.core.windows.net"

***ApplicationInsights Configuration:***
    *ApplicationInsights* to enable logging and monitoring.   

    "ApplicationInsights": {
        "LogLevel": {
        "Default": "Trace",
        "System": "Trace",
        "Microsoft": "Trace",
        "Microsoft.Hosting.Lifetime": "Information",
        "System.Net.Http.HttpClient": "Trace"
        }
    }
    "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "System": "Trace",
      "Microsoft": "Trace",
      "Microsoft.Hosting.Lifetime": "Information",
      "System.Net.Http.HttpClient": "Trace"
     }
    }

## (Azure AD Protected) Classifier API Configurations
   Configurations to connect

    {
      "AzureAd": {
        "Instance": "https://login.microsoftonline.com/",
        "TenantId": "770a2450-0227-4c62-90c7-4e38537f1102",
        "ClientId": "Enter_the_Application_Id_Here",
        "ClientCredentials": [
          {
            "SourceType": "ClientSecret",
            "ClientSecret": "Enter_the_Client_Secret_Here"
          }
        ],
        "CallbackPath": "/signin-oidc"
      },
      "ClassifierApi": {
        "BaseUrl": "http://localhost:5083/",
        "RelativePath": "api/vocabulary",
        "RequestAppToken": true,
        "Scopes": [ "api://Enter_the_Classifier_Api_Application_Id_Here/.default" ]
      },

## Feature Management
 
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

## ML Confidence Threshold Configurations

  - "AssetConfidenceThreshold": "0.8"
  - "PressureConfidenceThreshold": "0.8"
  - "BenefitConfidenceThreshold": "0.8"
  - "ValuationConfidenceThreshold": "0.6"
  - "CategoryConfidenceThreshold": "0.4"
  - "SubCategoryConfidenceThreshold": "0.9"


## Helm Chart Variables

The variables on helm Chart value file (*ncea-enricher\values\values.yaml*) will be replaced during Helm Deploy with environment specific values

| Variable name               | Variable Group              | Notes                                                               |
| ----------------------------|-----------------------------|---------------------------------------------------------------------|
| containerRepositoryFullPath | enricherServiceVariables    |                                                                     |
| imageTag                    |                             |  imageTag value is calculated dynamically variables-global.yml file |
| serviceAccountEnricher      | enricherServiceVariables    |                                                                     |
| serviceBusHostName          | azureVariables              |                                                                     |
| keyVaultUri                 | azureVariables              |                                                                     |
| blobStorageUri              | azureVariables              |                                                                     |
| autoScalingEnabled          | enricherServiceVariables    |                                                                     |
| autoScalingReplicas         | enricherServiceVariables    |                                                                     | 


## Pipeline Configurations

### Pipeline Setup

- azure-pipelines.yaml

- ***Stage : Build***
    - steps-build-and-test.yaml
        - task: UseDotNet@2 | version: '8.x'
        - task: DotNetCoreCLI@2 | command: 'restore'
        - task: DotNetCoreCLI@2 | command: 'build'
        - task: DotNetCoreCLI@2 | command: 'dotnet test'
        - task: PublishCodeCoverageResults@1 | codeCoverageTool: 'Cobertura'
        - task: SonarCloudAnalyze@1
        - task: SonarCloudPublish@1
        
    - steps-build-and-push-docker-images.yaml
        - task: AzureCLI@2 | To build and push docker images to DEV ACR
        
    - steps-package-and-push-helm-charts.yaml
        - task: HelmDeploy@0 | command: package
        - task: PublishPipelineArtifact@1 | Saves the Helm Chart as Pipeline Artifact

- ***Stage : dev***
    - steps-deploy-helm-charts.yaml
        - task: DownloadPipelineArtifact@2 | Downloads Helm Chart
        - task: ExtractFiles@1 | Extracts files from Helm Chart
        - task: HelmDeploy@0 | command: 'upgrade'

- ***Stage : tst***
    - steps-deploy-helm-charts.yaml
        - task: DownloadPipelineArtifact@2 | Downloads Helm Chart
        - task: ExtractFiles@1 | Extracts files from Helm Chart
        - task: HelmDeploy@0 | command: 'upgrade' 

- ***Stage : pre***
    - steps-import-docker-images.yaml
        - task: AzureCLI@2 | Import Docker Image from Dev ACR to Pre ACR
    
    - steps-deploy-helm-charts.yaml
        - task: DownloadPipelineArtifact@2 | Downloads Helm Chart
        - task: ExtractFiles@1 | Extracts files from Helm Chart
        - task: HelmDeploy@0 | command: 'upgrade' 
    
### Service Connections
- **dev**: AZR-NCE-DEV1
- **tst**: AZR-NCE-TST
- **pre**: AZR-NCE-PRE

### Build / Deployment Agents
- **dev** | **tst** : DEFRA-COMMON-ubuntu2204-SSV3
- **pre**           : DEFRA-COMMON-ubuntu2204-SSV5

### Variable Groups

***pipelineVariables***

    - acrConatinerRegistry
    - acrContainerRegistryPre
    - acrContainerRegistryPreShort
    - acrContainerRegistryDevResourceId
    - acrContainerRepositoryEnricher
    - acrName
    - acrUser
    - acrResourceGroupName
    - azureSubscriptionDev
    - azureSubscriptionTest
    - azureSubscriptionPre
    - sonarCloudOrganization
    - sonarProjectKeyEnricher
    - sonarProjectNameEnricher

***azureVariables-[dev/test/sandbox/...]***

    - aksNamespace
    - aksResourceGroupName
    - acrResourceGroupName
    - aksClusterName    
    - keyVaultUri
    - serviceBusHostName
    - blobStorageUri
    - fileShareClientUri
    - storageAccountName
    - storageAccountResourceGroup
    - storageAccountFilePrivateEndpointFqdn 

***EnricherServiceVariables-[dev/test/sandbox/...]***

    - autoScalingEnabled
    - autoScalingReplicas
    - containerRepostitoryFullPath
    - serviceAccountEnricher