using BotApp.Luis.Router.Domain;
using BotApp.Luis.Router.Domain.ServiceDiscovery;
using BotApp.Luis.Router.Domain.Settings;
using BotApp.Luis.Router.HostedServices.ServiceDiscovery;
using Consul;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotApp.Luis.Router
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

            Settings.AuthorizationKey = Configuration.GetSection("ApplicationSettings:AuthorizationKey")?.Value;

            try
            {
                var apps = new List<LuisAppRegistration>();
                Configuration.GetSection("LuisAppRegistrations").Bind(apps);
                Settings.LuisAppRegistrations = apps;

                Settings.LuisServices = new Dictionary<string, Microsoft.Bot.Builder.AI.Luis.LuisRecognizer>();
                foreach (LuisAppRegistration app in Settings.LuisAppRegistrations)
                {
                    var luis = new LuisApplication(app.LuisAppId, app.LuisAuthoringKey, app.LuisEndpoint);
                    var recognizer = new LuisRecognizer(luis);
                    Settings.LuisServices.Add(app.LuisName, recognizer);
                }
            }
            catch
            {
                throw new Exception("There was an exception loading LUIS apps");
            }

            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, ConsulHostedService>();
            services.Configure<ConsulConfig>(Configuration.GetSection("ConsulConfig"));
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                var address = Configuration["ConsulConfig:Address"];
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
            ConsulClient client = new ConsulClient(consulConfig =>
            {
                consulConfig.Address = new Uri(Configuration["ConsulConfig:Address"]);
            });

            await client.Agent.ServiceDeregister(ConsulHostedService.RegistrationID);
        }
    }
}