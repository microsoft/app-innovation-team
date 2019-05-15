FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY ["BotApp.LUIS.Middleware.Gateway/BotApp.LUIS.Middleware.Gateway.csproj", "BotApp.LUIS.Middleware.Gateway/"]
RUN dotnet restore "BotApp.LUIS.Middleware.Gateway/BotApp.LUIS.Middleware.Gateway.csproj"
COPY . .
WORKDIR "/src/BotApp.LUIS.Middleware.Gateway"
RUN dotnet build "BotApp.LUIS.Middleware.Gateway.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "BotApp.LUIS.Middleware.Gateway.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "BotApp.LUIS.Middleware.Gateway.dll"]