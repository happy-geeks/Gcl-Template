using Serilog;

var builder = WebApplication.CreateBuilder(args);

// First set the base settings for the application.
builder.Configuration.AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true);

// Get the base directory for secrets and then load the secrets file from that directory.
var secretsBasePath = builder.Configuration.GetSection("GCL").GetValue<string>("SecretsBaseDirectory");
if (!String.IsNullOrEmpty(secretsBasePath))
{
    builder.Configuration
        .AddJsonFile($"{secretsBasePath}appsettings-secrets.json", false, false)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true);
}

// Replace the placeholder with the project name.
var logFileSection = builder.Configuration.GetSection("Serilog:WriteTo");
var projectName = Assembly.GetEntryAssembly()?.GetName()?.Name ?? "Unknown";
var sections = logFileSection.GetChildren().Where(section => (section["Args:path"] ?? "").Contains("{fileName}")).ToList();
sections.ForEach(section => section["args:path"] = section["args:path"]!.Replace("{fileName}", $"{projectName}-{builder.Environment.EnvironmentName}-"));

// Configure Serilog.
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

Log.Logger.Debug($"----- Project {projectName} has just been (re)started. -----");

builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());
builder.Services.AddGclServices(builder.Configuration);
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Add all GCL middlewares and other things the GCL needs,
// such as controller endpoints, routing, static files middleware etc.
app.UseGclMiddleware(builder.Environment);

app.Run();