using BotApp.Extensions.Common.Consul.Helpers;
using BotApp.Extensions.Common.KeyVault.Helpers;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BotApp.Luis.Router.Identity
{
    public class Startup
    {
        public static string EnvironmentName { get; set; }
        public static string ContentRootPath { get; set; }
        public static string EncryptionKey { get; set; }
        public static string ApplicationCode { get; set; }

        public Startup(IHostingEnvironment env)
        {
            // Specify the environment name
            EnvironmentName = env.EnvironmentName;

            // Specify the content root path
            ContentRootPath = env.ContentRootPath;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            Settings.AuthorizationKey = Configuration.GetSection("ApplicationSettings:AuthorizationKey")?.Value;
            Settings.KeyVaultEncryptionKey = Configuration.GetSection("ApplicationSettings:KeyVaultEncryptionKey")?.Value;
            Settings.KeyVaultApplicationCode = Configuration.GetSection("ApplicationSettings:KeyVaultApplicationCode")?.Value;

            // Add Application Insights services into service collection
            services.AddApplicationInsightsTelemetry();

            // Add the standard telemetry client
            services.AddSingleton<TelemetryClient, TelemetryClient>();

            // Adding EncryptionKey and ApplicationCode
            using (KeyVaultHelper keyVaultHelper = new KeyVaultHelper(EnvironmentName, ContentRootPath))
            {
                Settings.KeyVaultEncryptionKey = Configuration.GetSection("ApplicationSettings:KeyVaultEncryptionKey")?.Value;
                EncryptionKey = keyVaultHelper.GetVaultKeyAsync(Settings.KeyVaultEncryptionKey).Result;

                Settings.KeyVaultApplicationCode = Configuration.GetSection("ApplicationSettings:KeyVaultApplicationCode")?.Value;
                ApplicationCode = keyVaultHelper.GetVaultKeyAsync(Settings.KeyVaultApplicationCode).Result;
            }

            // Adding Consul hosted service
            using (ConsulHelper consulHelper = new ConsulHelper(EnvironmentName, ContentRootPath))
            {
                consulHelper.Initialize(services, Configuration);
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = "https://BotApp.Luis.Router.Identity",
                        ValidAudience = "https://BotApp.Luis.Router.Identity",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.AuthorizationKey))
                    };
                });

            services.AddCors(o => o.AddPolicy("AllowAllPolicy", options =>
            {
                options.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            applicationLifetime.ApplicationStopping.Register(OnApplicationStopping);

            app.UseDefaultFiles()
                .UseStaticFiles();

            app.UseCors("AllowAllPolicy");

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc();
        }

        private async void OnApplicationStopping()
        {
            using (ConsulHelper consulHelper = new ConsulHelper(EnvironmentName, ContentRootPath))
            {
                await consulHelper.Stop();
            }
        }
    }
}