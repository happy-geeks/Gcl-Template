using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
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
            if (!string.IsNullOrEmpty(secretsBasePath))
            {
                builder
                    .AddJsonFile($"{secretsBasePath}appsettings-secrets.json", false, false);

                // Build the final configuration with all combined settings.
                Configuration = builder.Build();
            }

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();
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

            // Enable sessions and set the default timeout to 30 minutes.
            services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(30); });

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