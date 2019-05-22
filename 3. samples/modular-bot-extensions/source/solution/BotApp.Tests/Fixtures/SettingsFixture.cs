using BotApp;
using BotApp.Extensions.Common.KeyVault.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace IBE.Tests.Fixtures
{
    public class SettingsFixture : IDisposable
    {
        public SettingsFixture()
        {
            // Specify the environment name
            Startup.EnvironmentName = "UnitTesting";

            // Specify the content root path
            Startup.ContentRootPath = Directory.GetCurrentDirectory();

            var config = new ConfigurationBuilder()
              .AddJsonFile($"appsettings.{Startup.EnvironmentName}.json", optional: true, reloadOnChange: true)
              .Build();

            Settings.KeyVaultEncryptionKey = config.GetSection("ApplicationSettings:KeyVaultEncryptionKey")?.Value;
            Settings.KeyVaultApplicationCode = config.GetSection("ApplicationSettings:KeyVaultApplicationCode")?.Value;

            // Adding EncryptionKey and ApplicationCode
            KeyVaultService keyVaultService = new KeyVaultService(Startup.EnvironmentName, Startup.ContentRootPath);
            Startup.EncryptionKey = keyVaultService.GetVaultKeyAsync(Settings.KeyVaultEncryptionKey).Result;
            Startup.ApplicationCode = keyVaultService.GetVaultKeyAsync(Settings.KeyVaultApplicationCode).Result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
        }
    }
}