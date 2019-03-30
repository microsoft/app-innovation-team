FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY ["RF.Identity.Api/RF.Identity.Api.csproj", "RF.Identity.Api/"]
RUN dotnet restore "RF.Identity.Api/RF.Identity.Api.csproj"
COPY . .
WORKDIR "/src/RF.Identity.Api"
RUN dotnet build "RF.Identity.Api.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "RF.Identity.Api.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "RF.Identity.Api.dll"]
