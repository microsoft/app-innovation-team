FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY ["RF.ContentSearch.App/RF.ContentSearch.App.csproj", "RF.ContentSearch.App/"]
RUN dotnet restore "RF.ContentSearch.App/RF.ContentSearch.App.csproj"
COPY . .
WORKDIR "/src/RF.ContentSearch.App"
RUN dotnet build "RF.ContentSearch.App.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "RF.ContentSearch.App.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "RF.ContentSearch.App.dll"]