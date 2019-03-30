FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY ["RF.Contracts.Api/RF.Contracts.Api.csproj", "RF.Contracts.Api/"]
RUN dotnet restore "RF.Contracts.Api/RF.Contracts.Api.csproj"
COPY . .
WORKDIR "/src/RF.Contracts.Api"
RUN dotnet build "RF.Contracts.Api.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "RF.Contracts.Api.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "RF.Contracts.Api.dll"]
