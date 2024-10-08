# Welcome to the NCEA Enricher ETL Service Repository

This is the code repository for the NCEA Metadata Enricher ETL Service codebase.

## Process Flow

- Receive message from **mapped-queue**
- If the MessageType is **Metadata**, 
    - Read metadata file content from mapper staging container in Azure Blob Storage ( *jncc-mapper-staging | medin-mapper-staging* )(which saved by Mapper service) based on File Identifier
    - Enrich the MDC formated metadata xml based on ML Trained Models
    - Save the enriched xml file into Azure File Share (**FileShare**: */metadata-import* **FolderPath**: *jncc-new | medin-new*)
- If the MessageType is **End**,
    - Move the enriched xml files from previous run ( **/metadata-import/jncc | /metadata-import/medin** ) to back folder in file share ( **/metadata-import/jncc-backup | /metadata-import/medin-backup** )
    - Move the newly enriched xml files (from **/metadata-import/jncc-new | /metadata-import/medin-new**) to respective datasource folders ( **/metadata-import/jncc | /metadata-import/medin** ) in FileShare

## ML Prediction Flow

- Predict Theme 
    - Binary Prediction
    - Finalize the prediction based on Theme Prediction Score and Confidence Threshold Guideline value for the Theme
    - If the Theme Prediction Score is less than Confidence Threshold, then the respectve Theme prediction will be ignored
- Predict Category
    - Predict the Category for each Predicted Theme
    - If the Category Prediction Score is less than the Category Confidence Threshold, then the respectve Category prediction will be ignored
- Predict Subcategory
    - Predict the SubCategory for each Predicted Theme and Predicted Category
    - If the SubCategory Prediction Score is less than the SubCategory Confidence Threshold, then the respectve SubCategory prediction will be ignored
- Consolidate the Predicted Classifiers (Themes, Categories and SubCategories), build Classifier Hierarchy, construct Classifier XML nodes and Update the same in MDC xml object


# Prerequisites

Before proceeding, ensure you have the following installed:

- .NET 8 SDK: You can download and install it from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0).


# Configurations

## Azure Dependencies
   
***ServiceBus Configurations:***
    *ServiceBusHostName* to connect to ServiceBus, to send messages in servicebus queues and to dynamically create queues, if the *DynamicQueueCreation* is set to *True*   

    "ServiceBusHostName": "[Azure ServiceBus Namespace].servicebus.windows.net"
    "HarvesterQueueName": "harvested-queue"
    "MapperQueueName": "mapped-queue",
    "DynamicQueueCreation": true,

***KeyVault Configurations:***
    *KeyVaultUri* to access Azure KeyVault and to access secrets and connection strings.   

    "KeyVaultUri": "https://[Azure KeyVault Name].vault.azure.net/"

***BlobStorage Configuration:***
    *BlobStorageUri* to connect to Azure Blob Storage, to create containers per DataSource and to Save the XML files for the respective data source.
       
    "BlobStorageUri": "https://[Azure Storage Account Name].blob.core.windows.net"

***FileShare Configuration:***
    *FileShareName* to connect Azure File Share to Create, Move and Delete Enriched XML Files in FileShare.   

    "FileShareName": "/metadata-import"

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

| Variable name                         | Variable Group              | Notes                                                               |
| --------------------------------------|-----------------------------|---------------------------------------------------------------------|
| containerRepositoryFullPath           | enricherServiceVariables    |                                                                     |
| imageTag                              |                             |  imageTag value is calculated dynamically variables-global.yml file |
| serviceAccountEnricher                | enricherServiceVariables    |                                                                     |
| serviceBusHostName                    | azureVariables              |                                                                     |
| keyVaultUri                           | azureVariables              |                                                                     |
| blobStorageUri                        | azureVariables              |                                                                     |
| autoScalingEnabled                    | enricherServiceVariables    |                                                                     |
| autoScalingReplicas                   | enricherServiceVariables    |                                                                     | 
| storageAccountName                    | azureVariables              |                                                                     |
| storageAccountResourceGroup           | azureVariables              |                                                                     |
| storageAccountFilePrivateEndpointFqdn | azureVariables              |                                                                     |

Enricher uses Azure File Share as a Persistent Volume (PV) and Persistent Volume Claim (PVC)

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
    - classifierApiBaseUri
    - containerRepostitoryFullPath
    - serviceAccountEnricher

## For Future reference

- At present, enricher service connects with Classifier API using API Key Authentication. And the API keys are stored on Azure KeyVaults in respective environments.

- In future, if Classifier API swithched to Azure AD/ OAUTH then use the for following AppSettings. 

- Futher details available on User Story [448011](https://dev.azure.com/defragovuk/DEFRA-NCEA/_workitems/edit/448011): Implement OAUTH Security for Classifier API.

- You can find the branch name and draft PR details in the comment section of this user story

### (Azure AD Protected) Classifier API Configurations
   
   Configurations to connect classifier api with AzureAD authentication. Client Id and Client Secrets are stored Azure KeyVault and dynamically replaced during runtime. And the Tenant Id will be set as via Azure Pipleine variables during deployment.
    
    "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "Enter_the_AzureADTenanat_Id_Here",
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
    "AzureADTenantId": "Enter_the_Tenant_Id_Here",