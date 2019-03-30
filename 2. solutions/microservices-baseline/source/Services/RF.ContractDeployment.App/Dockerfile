FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY ["RF.ContractDeployment.App/RF.ContractDeployment.App.csproj", "RF.ContractDeployment.App/"]
RUN dotnet restore "RF.ContractDeployment.App/RF.ContractDeployment.App.csproj"
COPY . .
WORKDIR "/src/RF.ContractDeployment.App"
RUN dotnet build "RF.ContractDeployment.App.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "RF.ContractDeployment.App.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "RF.ContractDeployment.App.dll"]