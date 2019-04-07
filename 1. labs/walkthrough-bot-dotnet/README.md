## Intro

The walkthrough-bot-dotnet repo is a set of 3 laboratories designed to help you learn the following:

* How to build a bot from zero using Bot Builder V4 for .Net Core.

* How to build a CI/CD pipeline using Azure DevOps.

* How to build a container image for the bot and orchestrate its containers in Kubernetes.

## Prerequisites

1. An active Azure subscription. There are several ways you can procure one:
* [Azure Free Account](https://azure.microsoft.com/en-us/free/)
* [Visual Studio Dev Essentials](https://visualstudio.microsoft.com/dev-essentials/)
* [Monthly Azure credit for Visual Studio subscribers](https://azure.microsoft.com/en-us/pricing/member-offers/credit-for-visual-studio-subscribers/)
2. [VS Code](https://code.visualstudio.com/) o [Visual Studio 2019 Community](https://visualstudio.microsoft.com/vs/community/)
3. Azure DevOps free account (https://dev.azure.com/)
4. .Net Core 2.2 Installed (https://www.microsoft.com/net/download)
5. Git for Windows, Linux or MacOS are optional (https://git-scm.com/downloads)
6. Bot Framework V4 Emulator (https://github.com/Microsoft/BotFramework-Emulator/releases/tag/v4.1.0)
7. Docker Desktop (https://www.docker.com/get-started). For for older Mac and Windows systems that do not meet the requirements of [Docker Desktop for Mac](https://docs.docker.com/docker-for-mac/) and [Docker Desktop for Windows](https://docs.docker.com/docker-for-windows/) you could use [Docker Toolbox](https://docs.docker.com/toolbox/toolbox_install_windows/).

## Azure resources used in laboratories

1. [Azure Web App Bot](https://azure.microsoft.com/en-us/services/bot-service/)
2. [Language Understanding LUIS](https://azure.microsoft.com/en-us/services/cognitive-services/language-understanding-intelligent-service/)
3. [Translator](https://azure.microsoft.com/en-us/services/cognitive-services/translator-text-api/)
4. [Container Registry](https://azure.microsoft.com/en-us/services/container-registry/)
5. [Kubernetes Service](https://azure.microsoft.com/en-us/services/kubernetes-service/)
6. [Azure DevOps](https://azure.microsoft.com/en-us/services/devops/)

## Laboratories

1) [Programming the Bot](README-BotBuilderV4.md)
2) [Adding CI/CD pipelines to the Bot using Azure DevOps](README-AzDevOps.md)
3) [Bot on Kubernetes](README-Kubernetes.md)
