using Consul;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using RF.ContentSearch.Api.AuthorizationRequirements;
using RF.ContentSearch.Api.Domain.AuthorizationRequirements;
using RF.ContentSearch.Api.Domain.ServiceDiscovery;
using RF.ContentSearch.Api.Domain.Settings;
using RF.ContentSearch.Api.HostedServices.ServiceDiscovery;
using System;
using System.Collections.Generic;

namespace RF.ContentSearch.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ApplicationSettings.ConnectionString = Configuration.GetSection("ApplicationSettings:ConnectionString")?.Value;
            ApplicationSettings.DatabaseId = Configuration.GetSection("ApplicationSettings:DatabaseId")?.Value;
            ApplicationSettings.ContractCollection = Configuration.GetSection("ApplicationSettings:ContractCollection")?.Value;
            ApplicationSettings.RabbitMQUsername = Configuration.GetSection("ApplicationSettings:RabbitMQUsername")?.Value;
            ApplicationSettings.RabbitMQPassword = Configuration.GetSection("ApplicationSettings:RabbitMQPassword")?.Value;
            ApplicationSettings.RabbitMQHostname = Configuration.GetSection("ApplicationSettings:RabbitMQHostname")?.Value;
            ApplicationSettings.RabbitMQPort = Convert.ToInt16(Configuration.GetSection("ApplicationSettings:RabbitMQPort")?.Value);
            ApplicationSettings.ContractDeploymentQueueName = Configuration.GetSection("ApplicationSettings:ContractDeploymentQueueName")?.Value;
            ApplicationSettings.KeyVaultCertificateName = Configuration.GetSection("ApplicationSettings:KeyVaultCertificateName")?.Value;
            ApplicationSettings.KeyVaultClientId = Configuration.GetSection("ApplicationSettings:KeyVaultClientId")?.Value;
            ApplicationSettings.KeyVaultClientSecret = Configuration.GetSection("ApplicationSettings:KeyVaultClientSecret")?.Value;
            ApplicationSettings.KeyVaultIdentifier = Configuration.GetSection("ApplicationSettings:KeyVaultIdentifier")?.Value;
            ApplicationSettings.KeyVaultEncryptionKey = Configuration.GetSection("ApplicationSettings:KeyVaultEncryptionKey")?.Value;

            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, ConsulHostedService>();
            services.Configure<ConsulConfig>(Configuration.GetSection("ConsulConfig"));
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                var address = Configuration["ConsulConfig:address"];
                consulConfig.Address = new Uri(address);
            }));

            services.AddProtectWebApiWithMicrosoftIdentityPlatformV2(Configuration);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddAuthorization(options =>
            {
                var adGroupConfig = new List<AdGroupConfig>();
                Configuration.Bind("AdGroups", adGroupConfig);

                foreach (var adGroup in adGroupConfig)
                    options.AddPolicy(
                        adGroup.GroupName,
                        policy =>
                            policy.AddRequirements(new IsMemberOfGroupRequirement(adGroup.GroupName, adGroup.GroupId)));
            });

            services.AddSingleton<IAuthorizationHandler, IsMemberOfGroupHandler>();

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
            ConsulClient client = new ConsulClient(consulConfig =>
            {
                consulConfig.Address = new Uri(Configuration["ConsulConfig:address"]);
            });

            await client.Agent.ServiceDeregister(ConsulHostedService.RegistrationID);
        }
    }
}