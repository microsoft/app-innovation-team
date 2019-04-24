FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY ["BotApp.Gateway/BotApp.Gateway.csproj", "BotApp.Gateway/"]
RUN dotnet restore "BotApp.Gateway/BotApp.Gateway.csproj"
COPY . .
WORKDIR "/src/BotApp.Gateway"
RUN dotnet build "BotApp.Gateway.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "BotApp.Gateway.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "BotApp.Gateway.dll"]