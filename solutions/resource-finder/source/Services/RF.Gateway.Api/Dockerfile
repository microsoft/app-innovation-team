FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY ["RF.Gateway.Api/RF.Gateway.Api.csproj", "RF.Gateway.Api/"]
RUN dotnet restore "RF.Gateway.Api/RF.Gateway.Api.csproj"
COPY . .
WORKDIR "/src/RF.Gateway.Api"
RUN dotnet build "RF.Gateway.Api.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "RF.Gateway.Api.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "RF.Gateway.Api.dll"]