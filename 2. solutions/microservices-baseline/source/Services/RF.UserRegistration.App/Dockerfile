FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY ["RF.UserRegistration.App/RF.UserRegistration.App.csproj", "RF.UserRegistration.App/"]
RUN dotnet restore "RF.UserRegistration.App/RF.UserRegistration.App.csproj"
COPY . .
WORKDIR "/src/RF.UserRegistration.App"
RUN dotnet build "RF.UserRegistration.App.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "RF.UserRegistration.App.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "RF.UserRegistration.App.dll"]