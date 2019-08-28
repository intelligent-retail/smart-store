# Deploy resources with ARM template and Azure CLI

## About deployment environment

### Software to install

- Visual Studio
- Visual Studio Code
- Azure CLI
- Install the IoT Extension
- the `sqlcmd` utility
- Data migration tool ( `dt` command)
- git

### Check item

- Verify that you can log in to your Azure portal with your own Azure account
- Run `az account show` and check that you can log in with your own Azure account with Azure CLI

### About Visual Studio

This procedure is mainly used to deploy Azure Functions.

Please refer to the following when installing.

- [Downloads | IDE, Code, & Team Foundation Server | Visual Studio](https://visualstudio.microsoft.com/downloads/)

### About Visual Studio Code

This procedure is used to view and edit documents and source code.

Please refer to the following when installing.

- [Visual Studio Code-Code Editing. Redefined](https://code.visualstudio.com/#alt-downloads)

### About Azure CLI

It is Azure CLI that can be used cross platform. Used for deployment and provisioning.

Please refer to the following when installing.

- [Install Azure CLI | Microsoft Doc](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)

Also, since IoT extension functions are required, please install according to the following.  
    
    # Install the IoT extension
    az extension add --name azure-cli-iot-ext

### `sqlcmd` About the Utility

 `sqlcmd` The utility is used in provisioning scripts (_provision.ps1_ or _provision.sh_).

Please refer to the following when installing.

- Windows: [sqlcmd utility](https://docs.microsoft.com/en-us/sql/tools/sqlcmd-utility?view=sql-server-2017)
- Linux: [sqlcmd and bcp, install SQL Server command line tools on Linux] (https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-setup-tools?view= sql-server-2017)

### About the data migration tool ( `dt` command)

The Data migration tool ( `dt` command) is used to upload data to Cosmos DB. It is used in the script for provisioning (_provision.ps1_).

When installing, please extract the executable file referring to the following, and pass the path for `dt` .

- [Migrate data to Azure Cosmos DB using Data Migration Tool ( `dt` command)](https://docs.microsoft.com/en-us/azure/cosmos-db/import-data)

Note that this Data migration tool only works on Windows, so please use another method to upload data to Cosmos DB when working on Linux. The following is for reference.

- [Add sample data (using portal)](https://docs.microsoft.com/ja-jp/azure/cosmos-db/create-sql-api-dotnet#add-sample-data) -[Azure Cosmos DB Bulk Executor Library Overview](https://docs.microsoft.com/en-us/azure/cosmos-db/bulk-executor-overview)

## Deployment workflow

- Deploy with ARM template
- Provision using script
- Prepare the App Center
- Set API key for each Function
- Add settings to Application Settings in Azure Functions
- Each API key
- App Center URL and Key
- Deploy pos-service and box-service Functions in Visual Studio

## Deploy with ARM template

Deploy resources to Azure.

As we use Azure CLI, please prepare the environment referring to the following.

- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure)

After preparing the Azure CLI, please deploy the resources referring to the following.

### Deploy with PowerShell

    $ RESOURCE_GROUP = "<resource group name>"
    $ LOCATION = "japaneast"
    $ PREFIX = "<prefix string within 2 characters>"
    $ STOCK_SERVICE_SQL_SERVER_ADMIN_PASSWORD = "<sql server admin password>"

    $ TEMPLATE_URL = "https://raw.githubusercontent.com/intelligent-retail/smart-store/master/src/arm-template"

    # Create a resource group
    az group create `--name $ {RESOURCE_GROUP}`
    --location \$ {LOCATION}

    # Deploy resources in the created resource group
    az group deployment create `--resource-group $ {RESOURCE_GROUP}`
    --template-uri \$ {TEMPLATE_URL} /template.json `--parameters $ {TEMPLATE_URL} /parameters.json`
    --parameters `
        prefix = $ {PREFIX} `
        stockServiceSqlServerAdminPassword = $ {STOCK_SERVICE_SQL_SERVER_ADMIN_PASSWORD}

### Deploy with bash

    RESOURCE_GROUP = <resource group name>
    LOCATION = japaneast

    PREFIX = <prefix string within 2 characters>
    STOCK_SERVICE_SQL_SERVER_ADMIN_PASSWORD = <sql server admin password>
    TEMPLATE_URL = https://raw.githubusercontent.com/intelligent-retail/smart-store/master/src/arm-template

    # Create a resource group
        az group create \
        --name $ {RESOURCE_GROUP} \
        --location $ {LOCATION}

    # Deploy resources in the created resource group
        az group deployment create \
        --resource-group $ {RESOURCE_GROUP} \
        --template-uri $ {TEMPLATE_URL} /template.json \
        --parameters \$ {TEMPLATE_URL} /parameters.json \
        --parameters \
        prefix = $ {PREFIX} \
        stockServiceSqlServerAdminPassword = $ {STOCK_SERVICE_SQL_SERVER_ADMIN_PASSWORD}


## Provision with script

※ Variables are inherited from the previous section.

Perform provisioning using a script.

The script performs the following processing.

- Create table of SQL database
- Register the IoT Device for the IoT Hub
- Linking IoT Hub and BOX Management Service
- Create database, collection of each Cosmos DB

### Confirmation before execution

- Run `az extension show --name azure-cli-iot-ext` and check that the IoT extension is installed on Azure CLI
- Run `sqlcmd-?` to check that the sqlcmd utility is installed
- Run `dt` and check that the dt command is installed
- PowerShell script not available if not installed

### Provisioning with PowerShell

    # If you have not yet cloned the repository, clone it
    git clone https://github.com/intelligent-retail/smart-store.git

    # Move to repository directory
    cd smart-store

    # Pull if necessary
    git checkout master
    git pull

    # Check the program execution permission
    Get-ExecutionPolicy -List

    # If RemoteSigned does not hit CurrentUser above, do the following:
    Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

    # Perform provisioning
    .\src\arm-template\provision.ps1

### Provisioning with bash

_in preparation_

    # If you have not yet cloned the repository, clone it
    git clone https://github.com/intelligent-retail/smart-store.git

    # Move to repository directory
    cd smart-store

    # Pull if necessary
    git checkout master
    git pull

    # Perform provisioning
    ./src/arm-template/provision.sh

## Prepare the App Center

Prepare your push notification environment. Please refer to the following.

-[Build Push Notification Environment in App Center](/docs/appcenter.md)

## Set API key to each Function

Set the API key to Azure Functions.

The API key of Azure Functions can be set for the entire function or for each function individually. Here, to simplify the work, set the key of the same value to the whole function.

1. In the Azure portal, open one of the deployed Auzre Functions and open Function App Settings.
2. On the Function App Settings screen, click the Add New Host Key button for Host Keys (All Functions).
3. Enter `app` in the 'Name' field and click the 'Save' button to save. (The value is left blank and is automatically generated)
4. Once saved, click "Copy" in the "Action" column to copy the generated key.

Next, set the copied key to other Azure Functions.

1. Open other Azure Functions and move to the "Function App Settings" screen.
2. Click the Add New Host Key button for Host Key (All Functions).
3. Paste `app` in "Name", paste the copied key in "Value", and click the "Save" button to save.
4. Configure the other Azure Functions as well.

## Add Settings to Application Settings of Azure Functions

※ Variables are inherited from the previous section.

Add the API key and push notification information set in the previous section to Application Settings of Azure Functions.

In the following procedure, specify the API key set in Azure Functions for the variables below.

- `ITEM_MASTER_API_KEY`
- `STOCK_COMMAND_API_KEY`
- `POS_API_KEY`

In addition, please specify the key and URL of push notification in the following variables.

- `NOTIFICATION_API_KEY`
- `NOTIFICATION_URI`

For `NOTIFICATION_API_KEY` , paste the value obtained in the following procedure.

- Click the icon in the upper right corner of the App Center to open Account settings
- Open "API Tokens" under "Settings"
- Click the "New API token" button on the upper right
- Issue tokens according to the following
- Enter any descriptive text in "Description"
- Select `Full Access` in" Access "
- Click "Add new API token" button to issue
- Copy the token displayed in "Here's your API token." (Please note that it is displayed only once)

For `NOTIFICATION_URI` , paste the value obtained in the following procedure.

- Open an application created in the App Center
- Get the part of `{owner_name}` and `{app_name}` because the URL is configured as follows:

- `https://appcenter.ms/users/ {owner_name} / apps / {app_name}`

* `NOTIFICATION_URI` Replace `{owner_name}` and `{app_name}` in the following URL and set it to `NOTIFICATION_URI`
  - `https://api.appcenter.ms/v0.1/apps/ {owner_name} / {app_name} / push / notifications`

Please refer to the following for details.

- [Push | App Center API](https://openapi.appcenter.ms/#/push/Push_Send) -[How to find the app name and owner name from your app URL | App Center Help Center] (https://intercom.help/appcenter/general-questions/how-to-find-the-app-name-and- owner-name-from-your-app-url)

### Updating Azure Functions Application Settings with PowerShell

    # Set item-service and stock-service api key to pos-api

    $ ITEM_MASTER_API_KEY = "<item service api key>"
    $ STOCK_COMMAND_API_KEY = "<stock service command api key>"
    az functionapp config appsettings set `--resource-group $ {RESOURCE_GROUP}`
    --name \$ {PREFIX} -pos-api `--settings`

    ItemMasterApiKey = $ {ITEM_MASTER_API_KEY} `
    StockApiKey = $ {STOCK_COMMAND_API_KEY}

    # Set the api key and notification settings for pos-service to box-api

    $ POS_API_KEY = "<pos api key>"
    $ NOTIFICATION_API_KEY = "<app center push api key>"
    \$ NOTIFICATION_URI = "https://api.appcenter.ms/v0.1/apps/{owner_name}/{app_name}/push/notifications"
    az functionapp config appsettings set `--resource-group $ {RESOURCE_GROUP}`
    --name \$ {PREFIX} -box-api `--settings`

    NotificationApiKey = $ {NOTIFICATION_API_KEY} `
    NotificationUri = $ {NOTIFICATION_URI} `
    PosApiKey = $ {POS_API_KEY}

### Updating Application Settings for Azure Functions with bash

    # Set item-service and stock-service api key to pos-api

    ITEM_MASTER_API_KEY = <item service api key>
    STOCK_COMMAND_API_KEY = <stock service command api key>
    az functionapp config appsettings set \
    --resource-group $ {RESOURCE_GROUP} \
    --name $ {PREFIX} -pos-api \
    --settings \

    ItemMasterApiKey = $ {ITEM_MASTER_API_KEY} \
    StockApiKey = $ {STOCK_COMMAND_API_KEY}

    # Set the api key and notification settings for pos-service to box-api

    POS_API_KEY = <pos api key>
    NOTIFICATION_API_KEY = <app center push api key>
    NOTIFICATION_URI = https: //api.appcenter.ms/v0.1/apps/ {owner_name} / {app_name} / push / notifications
    az functionapp config appsettings set \
    --resource-group $ {RESOURCE_GROUP} \
    --name $ {PREFIX} -box-api \
    --settings \

    NotificationApiKey = $ {NOTIFICATION_API_KEY} \
    NotificationUri = $ {NOTIFICATION_URI} \
    PosApiKey = $ {POS_API_KEY}



## Deploy pos-service and box-service Functions in Visual Studio

In this section, we will use Visual Studio to deploy code for POS Management Service and BOX Management Service Functions.

### Deploy POS Management Service Code

1. Start Visual Studio
2. Open `src / pos-service / PosService.sln`
3. Right click on the `PosService` project of the `PosService` solution in the 'Solution Explorer' (or 'Solution Explorer')
4. Click "Publish" (or "Publish")
5. Open the "Azure Function App" (or "Azure Function App") tab in the "Pick a publish target" (or "Select publishing destination") dialog
6. Select "Select Existing" (or "Select an existing one") and check "Run from package file (recommended)" (or "Run from package file (recommended)")
7. Select "Create profile" (or "Create profile") from the pull-down on the lower right.
8. Operate "Subscription" "View" "Search" (or "Subscription" "View" "Search"), select "<PREFIX> -pos-api" as the Azure function of deployment destination, and " Click OK button
9. On the "Publish" screen, check that the created profile is displayed, and click the "Publish" button.

### Deploy Box Management Service Code

1. Open `src / box-service / BoxManagermentService.sln`
2. Right click on the `BoxManagementService` project of the `BoxManagementService` solution in the 'Solution Explorer'
3. Follow the same procedure as above and publish "<PREFIX> -box-api"

## Operation check

Please refer to the following document for operation check.

- [Operation check](/docs/operation-check.md)

## Remarks

### Preparation of various masters when not using script

Here, we will introduce how to prepare the master manually. If you can execute by script in [Provision using script] (#provision using script), please skip this section.

##### Preparation of integrated product master

There are many ways to operate Cosmos DB, so use them as appropriate.

- Import to Azure Cosmos DB -[Add sample data (using portal)](https://docs.microsoft.com/ja-jp/azure/cosmos-db/create-sql-api-dotnet#add-sample-data) -[Migrate data to Azure Cosmos DB using Data Migration Tool ( `dt` command)](https://docs.microsoft.com/en-us/azure/cosmos-db/import-data) -[Azure Cosmos DB Bulk Executor Library Overview](https://docs.microsoft.com/en-us/azure/cosmos-db/bulk-executor-overview)

Here's how to do the following tasks from the command line using Azure CLI and the data migration tool ( `dt` command).


    # Insert documents to item-service Cosmos DB

    ITEM_SERVICE_COSMOSDB_DATABASE = "00100"
    ITEM_SERVICE_COSMOSDB_DATABASE_THROUGHPUT = "400"
    ITEM_SERVICE_COSMOSDB_COLLECTION = "Items"
    ITEM_SERVICE_COSMOSDB_COLLECTION_PARTITIONKEY = "/ storeCode"

    ITEM_SERVICE_COSMOSDB = az cosmosdb list \
    --resource-group $ {RESOURCE_GROUP} \
        --query "[? contains (@. name, 'item') == ` ` true``] .name" \
        --output tsv
    ITEM_SERVICE_COSMOSDB_CONNSTR = $ (az cosmosdb list-connection-strings \
    --resource-group $ {RESOURCE_GROUP} \
        --name $ {ITEM_SERVICE_COSMOSDB} \
    --query "connectionStrings [0] .connectionString" \
    --output tsv)

    <your-dt-command-path> /dt.exe \

    / s: JsonFile \
    /s.Files:.\\src\\arm-template\\sample-data\\public\\item-service\\itemMasterSampleData.json \
    / t: DocumentDB \
    /t.ConnectionString: "$ {ITEM_SERVICE_COSMOSDB_CONNSTR}; Database = $ {ITEM_SERVICE_COSMOSDB_DATABASE};" \
    /t.Collection: $ {ITEM_SERVICE_COSMOSDB_COLLECTION} \
    /t.PartitionKey: $ {ITEM_SERVICE_COSMOSDB_COLLECTION_PARTITIONKEY} \
    /t.CollectionThroughput: $ {ITEM_SERVICE_COSMOSDB_DATABASE_THROUGHPUT}

#### Preparation for including images in product data

Upload the image if necessary. Although the explanation is made for sample images and import files, please replace as appropriate for reference.

1. Upload png images stored under `sample-data / public / item-service / images` directory to Azure Blog Storage
2. Reflect uploaded URL in JSON data for import

There are various ways to upload images, so use them as appropriate.

- Upload to Azure Blob Storage -[Upload, download and list blobs using Azure portal](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-portal)
- Create a blob in object storage using Azure Storage Explorer (https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-storage-explorer) -[Upload, download and list blobs using Azure CLI](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-cli)

Here we introduce the method using Azure CLI.

    # Set variables following above, if you did not set them

    # Upload assets images
    ASSETS_BLOB_STORAGE_NAME = $ (az storage account list \
        --resource-group $ {RESOURCE_GROUP} \
    --query "[? contains (@. name, 'assets') == \ ` true \ ` ] .name" \
    --output tsv)
    ASSETS_BLOB_STORAGE_CONTAINER = $ (az storage container list \
        --account-name $ {ASSETS_BLOB_STORAGE_NAME} \
    --query "[0] .name" \
    --output tsv)
    ASSETS_BLOB_STORAGE_CONNSTR = $ (az storage account show-connection-string \
        --name $ {ASSETS_BLOB_STORAGE_NAME} \
    --query "connectionString" \
    --output tsv)

    az storage blob upload-batch \
    --connection-string $ {ASSETS_BLOB_STORAGE_CONNSTR} \
        --destination $ {ASSETS_BLOB_STORAGE_CONTAINER} \
    --source src / arm-template / sample-data / public / item-service / images \
    --pattern "\* .png"

    # Get endpoint of assets storage
    ASSETS_BLOB_STORAGE_URL = $ (az storage account show \
        --name $ {ASSETS_BLOB_STORAGE_NAME} \
    --query "primaryEndpoints.blob" \
    --output tsv)

    # Set image paths into the source data
    sed -i -e "s | https: //sample.blob.core.windows.net/ | \$ {ASSETS_BLOB_STORAGE_URL} | g" src / arm-template / sample-data / public / item-service / itemMasterSampleData.json

Now that the image data has been reflected in the data for import of the product master, please return to [Preparation of integrated product master] (#Preparation of integrated product master) and carry out the procedure.

#### Preparation of Master of Box Management Service / POS Service

Prepare a master of Box management service and POS service in Azure Cosmos DB, and register data as needed.

Next, register the data.  
Data migration is provided in various ways. Here's how to do the following tasks from the command line using Azure CLI and the data migration tool ( `dt` command). We have prepared an import file, but please read it as appropriate.

- [Migrate data to Azure Cosmos DB using Data Migration Tool ( `dt` command)](https://docs.microsoft.com/en-us/azure/cosmos-db/import-data)

        # Set variables following above, if you did not set them
        POS_DB_ACCOUNT_NAME = $ {PREFIX} -pos-service
        BOX_DB_ACCOUNT_NAME = $ {PREFIX} -box-service
        POS_DB_NAME = 'smartretailpos'
        BOX_DB_NAME = 'smartretailboxmanagement'

        # Create connection string
        POS_SERVICE_COSMOSDB_CONNSTR = $ (az cosmosdb list-connection-strings \
            --resource-group $ {RESOURCE_GROUP} \
        --name $ {POS_DB_ACCOUNT_NAME} \
            --query "connectionStrings [0] .connectionString" \
            --output tsv)
        BOX_SERVICE_COSMOSDB_CONNSTR = $ (az cosmosdb list-connection-strings \
        --resource-group $ {RESOURCE_GROUP} \
            --name $ {BOX_DB_ACCOUNT_NAME} \
        --query "connectionStrings [0] .connectionString" \
        --output tsv)

        # Insert documents to Cosmos DB
        <your-dt-command-path> /dt.exe \
        / s: JsonFile \
        /s.Files:.\\src\\arm-template\\sample-data\\public\\pos-service\\PosMasters.json \
        / t: DocumentDB \
        /t.ConnectionString: "$ {POS_SERVICE_COSMOSDB_CONNSTR}; Database = $ {POS_DB_NAME};" \
        /t.Collection: PosMasters
        <your-dt-command-path> /dt.exe \
        / s: JsonFile \
        /s.Files:.\\src\\arm-template\\sample-data\\public\\box-service\\Skus.json \
        / t: DocumentDB \
        /t.ConnectionString: "$ {BOX_SERVICE_COSMOSDB_CONNSTR}; Database = $ {BOX_DB_NAME};" \
        /t.Collection:Skus
        <your-dt-command-path> /dt.exe \
        / s: JsonFile \
        /s.Files:.\\src\\arm-template\\sample-data\\public\\box-service\\Terminals.json \
        / t: DocumentDB \
        /t.ConnectionString: "$ {BOX_SERVICE_COSMOSDB_CONNSTR}; Database = $ {BOX_DB_NAME};" \
        /t.Collection:Terminals