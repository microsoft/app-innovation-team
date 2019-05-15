FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY ["BotApp/BotApp.csproj", "BotApp/"]
RUN dotnet restore "BotApp/BotApp.csproj"
COPY . .
WORKDIR "/src/BotApp"
RUN dotnet build "BotApp.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "BotApp.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "BotApp.dll"]