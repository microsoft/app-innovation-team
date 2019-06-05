FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY ["BotApp.Luis.Router.Identity/BotApp.Luis.Router.Identity.csproj", "BotApp.Luis.Router.Identity/"]
RUN dotnet restore "BotApp.Luis.Router.Identity/BotApp.Luis.Router.Identity.csproj"
COPY . .
WORKDIR "/src/BotApp.Luis.Router.Identity"
RUN dotnet build "BotApp.Luis.Router.Identity.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "BotApp.Luis.Router.Identity.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "BotApp.Luis.Router.Identity.dll"]
