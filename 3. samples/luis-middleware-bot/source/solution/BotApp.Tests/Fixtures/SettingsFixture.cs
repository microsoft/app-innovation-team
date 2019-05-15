using BotApp;
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
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddIniFile("config.ini", optional: false, reloadOnChange: true)
                .Build();

            var LuisName01 = config.GetSection("general:LuisName01").Value;
            TestSettings.LuisName01 = LuisName01;

            var LuisAppId01 = config.GetSection("general:LuisAppId01").Value;
            TestSettings.LuisAppId01 = LuisAppId01;

            var LuisAuthoringKey01 = config.GetSection("general:LuisAuthoringKey01").Value;
            TestSettings.LuisAuthoringKey01 = LuisAuthoringKey01;

            var LuisEndpoint01 = config.GetSection("general:LuisEndpoint01").Value;
            TestSettings.LuisEndpoint01 = LuisEndpoint01;

            var LuisName02 = config.GetSection("general:LuisName02").Value;
            TestSettings.LuisName02 = LuisName02;

            var LuisAppId02 = config.GetSection("general:LuisAppId02").Value;
            TestSettings.LuisAppId02 = LuisAppId02;

            var LuisAuthoringKey02 = config.GetSection("general:LuisAuthoringKey02").Value;
            TestSettings.LuisAuthoringKey02 = LuisAuthoringKey02;

            var LuisEndpoint02 = config.GetSection("general:LuisEndpoint02").Value;
            TestSettings.LuisEndpoint02 = LuisEndpoint02;

            var QnAName = config.GetSection("general:QnAName").Value;
            Settings.QnAName = QnAName;

            var QnAKbId = config.GetSection("general:QnAKbId").Value;
            Settings.QnAKbId = QnAKbId;

            var QnAEndpointKey = config.GetSection("general:QnAEndpointKey").Value;
            Settings.QnAEndpointKey = QnAEndpointKey;

            var QnAHostname = config.GetSection("general:QnAHostname").Value;
            Settings.QnAHostname = QnAHostname;

            var LuisMiddlewareUrl = config.GetSection("general:LuisMiddlewareUrl").Value;
            Settings.LuisMiddlewareUrl = LuisMiddlewareUrl;

            var KeyVaultCertificateName = config.GetSection("general:KeyVaultCertificateName").Value;
            Settings.KeyVaultCertificateName = KeyVaultCertificateName;

            var KeyVaultClientId = config.GetSection("general:KeyVaultClientId").Value;
            Settings.KeyVaultClientId = KeyVaultClientId;

            var KeyVaultClientSecret = config.GetSection("general:KeyVaultClientSecret").Value;
            Settings.KeyVaultClientSecret = KeyVaultClientSecret;

            var KeyVaultIdentifier = config.GetSection("general:KeyVaultIdentifier").Value;
            Settings.KeyVaultIdentifier = KeyVaultIdentifier;

            var KeyVaultEncryptionKey = config.GetSection("general:KeyVaultEncryptionKey").Value;
            Settings.KeyVaultEncryptionKey = KeyVaultEncryptionKey;

            var KeyVaultApplicationCode = config.GetSection("general:KeyVaultApplicationCode").Value;
            Settings.KeyVaultApplicationCode = KeyVaultApplicationCode;
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