## Intro

This bot has the specific purpose of demonstrate how we can expand the capabilities of QnA Maker bot with the natural language understanding service (LUIS).

If you have not any experience in Bot Builder V4, check: https://github.com/Microsoft/app-innovation-team/tree/master/labs/walkthrough-bot-dotnet

## Prerequisites

1. Active Azure subscription
2. VS Code o Visual Studio 2017 Community
3. Azure DevOps free account (https://dev.azure.com/)
4. .Net Core Installed (https://www.microsoft.com/net/download)
5. Git for Windows, Linux or MacOS (https://git-scm.com/downloads) (optional)
6. Bot Framework V4 Emulator (https://github.com/Microsoft/BotFramework-Emulator/releases/tag/v4.1.0)
7. Docker Community Edition (https://www.docker.com/get-started) (optional)

## Create LUIS and QnA Services

1. Create the LUIS app, check: https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-get-started-create-app

2. Import the LUIS application model: luis-qna-bot-dotnet\source\models\BotApp-LUIS.json

3. Create the QnA service and Knowledge base: 
    - https://docs.microsoft.com/en-us/azure/cognitive-services/qnamaker/how-to/set-up-qnamaker-service-azure
    - https://docs.microsoft.com/en-us/azure/cognitive-services/qnamaker/how-to/create-knowledge-base

4. Import the QnA Maker application model: luis-qna-bot-dotnet\source\models\BotApp-QnA.tsv

## Clone the project

`git clone https://github.com/Microsoft/app-innovation-team.git`

## Setup the project

Update the `luis-qna-bot-dotnet\source\bot-app\BotApp\appsettings.json` file in the root of the bot project.

Your appsettings.json file should look like this
```bash
{
  "MicrosoftAppId": "",
  "MicrosoftAppPassword": "",
  "BotVersion": "1.0",
  "TimeZone": "Central Standard Time (Mexico)", // if linux host the bot use: "America/Mexico_City",
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
  "QnAHostname01": "QNA_HOSTNAME"
}
```