using Consul;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RF.Contracts.Api.HostedService.ServiceDiscovery;
using RF.Contracts.Domain.Entities.ServiceDiscovery;
using System;
using System.Text;

namespace RF.Contracts.Api
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            Settings.AuthorizationKey = Configuration.GetSection("AuthorizationKey")?.Value;
            Settings.ConnectionString = Configuration.GetSection("ConnectionString")?.Value;
            Settings.DatabaseId = Configuration.GetSection("DatabaseId")?.Value;
            Settings.ContractCollection = Configuration.GetSection("ContractCollection")?.Value;
            Settings.RabbitMQUsername = Configuration.GetSection("RabbitMQUsername")?.Value;
            Settings.RabbitMQPassword = Configuration.GetSection("RabbitMQPassword")?.Value;
            Settings.RabbitMQHostname = Configuration.GetSection("RabbitMQHostname")?.Value;
            Settings.RabbitMQPort = Convert.ToInt16(Configuration.GetSection("RabbitMQPort")?.Value);
            Settings.ContractDeploymentQueueName = Configuration.GetSection("ContractDeploymentQueueName")?.Value;
            Settings.KeyVaultCertificateName = Configuration.GetSection("KeyVaultCertificateName")?.Value;
            Settings.KeyVaultClientId = Configuration.GetSection("KeyVaultClientId")?.Value;
            Settings.KeyVaultClientSecret = Configuration.GetSection("KeyVaultClientSecret")?.Value;
            Settings.KeyVaultIdentifier = Configuration.GetSection("KeyVaultIdentifier")?.Value;
            Settings.KeyVaultEncryptionKey = Configuration.GetSection("KeyVaultEncryptionKey")?.Value;

            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, ConsulHostedService>();
            services.Configure<ConsulConfig>(Configuration.GetSection("consulConfig"));
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                var address = Configuration["consulConfig:address"];
                consulConfig.Address = new Uri(address);
            }));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = "https://rf.identity.api",
                        ValidAudience = "https://rf.identity.api",
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
            ConsulClient client = new ConsulClient(consulConfig =>
            {
                consulConfig.Address = new Uri(Configuration["consulConfig:address"]);
            });

            await client.Agent.ServiceDeregister(ConsulHostedService.RegistrationID);
        }
    }
}