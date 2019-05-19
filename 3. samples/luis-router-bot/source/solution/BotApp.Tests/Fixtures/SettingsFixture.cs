using BotApp;
using BotApp.Extensions.Common.KeyVault.Helpers;
using BotApp.Tests;
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

            // Adding EncryptionKey and ApplicationCode
            using (KeyVaultHelper keyVaultHelper = new KeyVaultHelper(Startup.EnvironmentName, Startup.ContentRootPath))
            {
                Settings.KeyVaultEncryptionKey = config.GetSection("ApplicationSettings:KeyVaultEncryptionKey")?.Value;
                Startup.EncryptionKey = keyVaultHelper.GetVaultKeyAsync(Settings.KeyVaultEncryptionKey).Result;

                Settings.KeyVaultApplicationCode = config.GetSection("ApplicationSettings:KeyVaultApplicationCode")?.Value;
                Startup.ApplicationCode = keyVaultHelper.GetVaultKeyAsync(Settings.KeyVaultApplicationCode).Result;
            }
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