FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY ["RF.ContentApproval.App/RF.ContentApproval.App.csproj", "RF.ContentApproval.App/"]
RUN dotnet restore "RF.ContentApproval.App/RF.ContentApproval.App.csproj"
COPY . .
WORKDIR "/src/RF.ContentApproval.App"
RUN dotnet build "RF.ContentApproval.App.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "RF.ContentApproval.App.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "RF.ContentApproval.App.dll"]