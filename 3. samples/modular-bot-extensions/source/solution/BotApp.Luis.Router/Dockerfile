FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY ["BotApp.Luis.Router/BotApp.Luis.Router.csproj", "BotApp.Luis.Router/"]
RUN dotnet restore "BotApp.Luis.Router/BotApp.Luis.Router.csproj"
COPY . .
WORKDIR "/src/BotApp.Luis.Router"
RUN dotnet build "BotApp.Luis.Router.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "BotApp.Luis.Router.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "BotApp.Luis.Router.dll"]
