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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
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