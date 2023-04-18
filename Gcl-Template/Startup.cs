using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using GeeksCoreLibrary.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Gcl_Template
{
    public class Startup
    {
        public Startup(IWebHostEnvironment webHostEnvironment)
        {
            // First set the base settings for the application.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{webHostEnvironment.EnvironmentName}.json", true, true);

            // We need to build here already, so that we can read the base directory for secrets.
            Configuration = builder.Build();

            // Get the base directory for secrets and then load the secrets file from that directory.
            var secretsBasePath = Configuration.GetSection("GCL").GetValue<string>("SecretsBaseDirectory");
            if (!String.IsNullOrEmpty(secretsBasePath))
            {
                builder
                    .AddJsonFile($"{secretsBasePath}appsettings-secrets.json", false, false)
                    .AddJsonFile($"appsettings.{webHostEnvironment.EnvironmentName}.json", true, true);

                // Build the final configuration with all combined settings.
                Configuration = builder.Build();
            }

            // Replace the place holder with the project name.
            var logFileSection = Configuration.GetSection("Serilog:WriteTo");
            var projectName = Assembly.GetEntryAssembly()?.GetName()?.Name ?? "Unknown";
            var sections = logFileSection.GetChildren().Where(section => (section["Args:path"] ?? "").Contains("{fileName}")).ToList();
            sections.ForEach(section => section["args:path"] = section["args:path"]!.Replace("{fileName}", $"{projectName}-{webHostEnvironment.EnvironmentName}-"));

            // Configure Serilog.
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            Log.Logger.Debug($"----- Project {projectName} has just been (re)started. -----");
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Serilog.
            services.AddLogging((builder) => { builder.AddSerilog(); });

            // Add all GCL services.
            services.AddGclServices(Configuration);

            // Add MVC.
            services.AddControllersWithViews();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Add all GCL middle wares and other things the GCL needs, such as controller endpoints, routing, static files middle ware etc.
            app.UseGclMiddleware(env);
        }
    }
}