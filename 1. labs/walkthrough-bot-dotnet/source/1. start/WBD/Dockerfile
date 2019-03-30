FROM microsoft/dotnet:2.1.5-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1.403-sdk AS build
WORKDIR /src

COPY WBD.csproj ./
RUN dotnet restore ./WBD.csproj

COPY . .
RUN dotnet build WBD.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish WBD.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "WBD.dll"]