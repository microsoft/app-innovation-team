## Intro

This bot has the specific purpose of demonstrate how we can expand the capabilities of QnA Maker bot with the natural language understanding service (LUIS).

If you have not any experience in Bot Builder V4, check: https://github.com/Microsoft/app-innovation-team/tree/master/labs/walkthrough-bot-dotnet

## Services implemented

The solution is splitted in the following services:

- API Gateway (web)
- Discovery Service (web)
- Bot App (web)


## Technology stack used

- Docker
- Azure CosmosDB (DocumentDB and MongoDB)
- Consul
- Ocelot
- .NET Core

## Running the project (Prerequisites)

To run the project locally you need:

- Azure subscription
- Visual Studio 2017 / 2019
- Visual Studio Code (optional)
- Docker (latest version)
- Docker Compose (latest version)
- Net Core 2.2 SDK
- Git for Windows, Linux or MacOS (https://git-scm.com/downloads) (optional)
- Bot Framework V4 Emulator (https://github.com/Microsoft/BotFramework-Emulator/releases/tag/v4.1.0)
- Docker Community Edition (https://www.docker.com/get-started) (optional)
- Optional: you may want to install CTOP if you are using Linux/MacOS to manage containers from your terminal: https://github.com/bcicen/ctop

<b>Important:</b> The current microservices solution is optimized to run on Linux/MacOS, it means I’m using a specific configuration in the code to handle IO directly with libuv unix sockets, said that, I would suggest host Docker on MacOS or Linux VM, otherwise you will need to perform some adjustments in the code to add IIS service on Program.cs on each API exposed.

### Docker Compose

Docker Compose is a container orchestrator for deployments in one single node, you can scale and create memory boundaries cross the containerized services. To run the full project in one single execution you will need to have installed Docker Compose, this solution was tested on: version 1.23.2.

## Create LUIS and QnA Services

1. Create the LUIS app, check: https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-get-started-create-app

2. Import the LUIS application model: luis-qna-bot-dotnet\source\models\BotApp-LUIS.json

3. Create the QnA service and Knowledge base: 
    - https://docs.microsoft.com/en-us/azure/cognitive-services/qnamaker/how-to/set-up-qnamaker-service-azure
    - https://docs.microsoft.com/en-us/azure/cognitive-services/qnamaker/how-to/create-knowledge-base

4. Import the QnA Maker application model: luis-qna-bot-dotnet\source\models\BotApp-QnA.tsv

### Clone the repo

Clone the repo with the following GIT command: `git clone https://github.com/Microsoft/app-innovation-team.git`

Update the `intelligent-bot-experience\source\solution\BotApp\appsettings.Development.json` file in the root of the bot project.

Your appsettings.json file should look like this
```bash
{
  "MicrosoftAppId": "",
  "MicrosoftAppPassword": "",
  "BotConversationStorageConnectionString": "DOCUMENTDB_CONNECTIONSTRING",
  "BotConversationStorageKey": "DOCUMENTDB_STORAGEKEY",
  "BotConversationStorageDatabaseId": "DOCUMENTDB_DATABASEID (e.g. bot)",
  "BotConversationStorageUserCollection": "DOCUMENTDB_USERCOLLECTION (e.g. user)",
  "BotConversationStorageConversationCollection": "DOCUMENTDB_CONVERSATIONCOLLECTION (e.g. conversation)",
  "LuisName01": "LUIS_NAME",
  "LuisAppId01": "LUIS_APPID",
  "LuisAuthoringKey01": "LUIS_AUTHORINGKEY",
  "LuisEndpoint01": "LUIS_ENDPOINT",
  "QnAName01": "QNA_NAME",
  "QnAKbId01": "QNA_KBID",
  "QnAEndpointKey01": "QNA_ENDPOINT",
  "QnAHostname01": "QNA_HOSTNAME",
  "AzureWebJobsStorage": "AZURE_STORAGE_CONNECTION_STRING",
  "FaceAPIKey": "FACE_API_KEY",
  "FaceAPIZone": "FACE_API_ZONE(e.g. westus or southcentralus)",
  "LargeFaceListId": "LARGE_FACE_LIST_ID",
  "MongoDBConnectionString": "MONGODB_CONNECTION_STRING_OR_COSMOSDB_WITH_MONGODB_DATABASE",
  "MongoDBDatabaseId": "DATABASE_ID",
  "PersonCollection": "PERSON_COLLECTION",
  "ConsulConfig": {
    "address": "http://consul:8500",
    "serviceName": "bot-app",
    "serviceID": "bot-app",
    "serviceTag": "BotApp"
  }
}
```

### Docker Compose configuration

Review the docker compose file.

```
version: '2.4'

services:
  consul:
    image: consul:1.4.3
    mem_limit: 250M
    logging:
      driver: "json-file"
      options:
        max-size: "50m"
    hostname: consul
    ports:
      - "8500:8500"
      - "8600:8600"
    expose:
      - 8500
      - 8600
    restart: always
  gateway:
    image: gateway:1.0
    mem_limit: 500M
    logging:
      driver: "json-file"
      options:
        max-size: "50m"
    hostname: gateway
    environment:
        - ASPNETCORE_ENVIRONMENT=Development
    build:
      context: .
      dockerfile: BotApp.Gateway/Dockerfile
    ports:
      - "17070:80"
    restart: always
    depends_on:
      - consul
  bot-app:
    image: bot-app:1.0
    mem_limit: 250M
    logging:
      driver: "json-file"
      options:
        max-size: "50m"
    hostname: bot-app
    environment:
        - ASPNETCORE_ENVIRONMENT=Development
    build:
      context: .
      dockerfile: BotApp/Dockerfile
    ports:
      - "80"
    restart: always
    depends_on:
      - consul
```

### Docker Compose commands used

<b>docker-compose stop</b> 

In case you need to stop the containers.

<b>docker-compose rm</b>

In case you need to remove the containers.

<b>docker-compose up --build</b>

In case you need to up and build the services in the docker-compose.yml file.

<b>docker-compose up --build --scale service=NUM_INSTANCES</b>

In case you need to up, build and scale the services in the docker-compose.yml file.

<b>Important: </b> docker-compose commands must be executed in the same path level of the docker-compose.yml file.
