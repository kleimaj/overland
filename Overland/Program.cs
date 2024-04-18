using System.Net;
using Destructurama;
using FluentValidation;
using FluentValidation.AspNetCore;
using Marten;
using NpgsqlTypes;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using Weasel.Core;
using FormHelper;
using Joonasw.AspNetCore.SecurityHeaders;
using Marten.Services.Json;
using Serilog.Sinks.PostgreSQL.ColumnWriters;
using Overland.Models;
using Overland.Models.Settings;
using Overland.Services;
using Overland.Validators;

var tableName = "logs";
IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase> {
    { "message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
    { "message_template", new MessageTemplateColumnWriter(NpgsqlDbType.Text) },
    { "level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
    { "raise_date", new TimestampColumnWriter(NpgsqlDbType.Timestamp) },
    { "exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
    { "properties", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) },
    { "props_test", new PropertiesColumnWriter(NpgsqlDbType.Jsonb) }, {
        "machine_name",
        new SinglePropertyColumnWriter("MachineName", PropertyWriteMethod.ToString, NpgsqlDbType.Text, "l")
    }
};
var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var env = builder.Environment.EnvironmentName;
var appName = builder.Environment.ApplicationName;

builder.WebHost.ConfigureKestrel(kestrelServerOptions => {
    kestrelServerOptions.ConfigureHttpsDefaults(listenOptions => {
        kestrelServerOptions.Listen(IPAddress.Loopback, 5001);
        //kestrelServerOptions.Listen(IPAddress.Parse("172.28.1.5"), 5001); // nginx proxy manager
        kestrelServerOptions.Limits.MaxRequestBodySize = int.MaxValue; // if don't set default value is: 30 MB
    });
});
builder.Configuration.AddSecretsManager(configurator: options => {
    options.SecretFilter = entry => entry.Name.StartsWith($"{env}_{appName}");
    options.KeyGenerator = (_, s) => s
        .Replace($"{env}_{appName}_", string.Empty)
        .Replace("__", ":");
});

/*
IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();
    */

// Add services to the container.
builder.Services.AddResponseCaching();
builder.Services.AddCsp();
builder.Services.AddControllersWithViews().AddFormHelper(options => {
    options.EmbeddedFiles = true;
    options.ToastrDefaultPosition = ToastrPosition.TopRight;
});
builder.Services.AddSwaggerGen();

/*builder.Services.AddSwaggerGen();*/
builder.Services.Configure<DmsApiConfig>(builder.Configuration.GetSection(DmsApiConfig.Key));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.Key));

// This is the absolute, simplest way to integrate Marten into your
// .NET application with Marten's default configuration
builder.Services.AddMarten(options => {
    options.Connection(builder.Configuration.GetSection(DmsApiConfig.Key)["PostgresConnectionString"]);
    options.AutoCreateSchemaObjects = AutoCreate.All;
    options.UseDefaultSerialization(
        serializerType: SerializerType.SystemTextJson,
        enumStorage: EnumStorage.AsString,
        casing: Casing.CamelCase
    );
}).OptimizeArtifactWorkflow().InitializeWith().UseLightweightSessions();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddTransient<IValidator<Customer>, CustomerValidator>();
builder.Services.AddSingleton<IMartenService, MartenService>();
builder.Services.AddSingleton<IDmsLoggerService, DmsLoggerService>();

using var log = new LoggerConfiguration()
    //.MinimumLevel.Error()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .Destructure.JsonNetTypes()
    .WriteTo.PostgreSQL(builder.Configuration.GetSection(DmsApiConfig.Key)["PostgresConnectionString"], tableName,
        columnWriters, needAutoCreateTable: true)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(log);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseCsp(csp => {
    csp.AllowBaseUri
        .FromSelf();
    csp.AllowScripts
        .FromAnywhere()
        //.AddNonce()
        .AllowUnsafeInline(); //Fallback for browsers that don't support nonce
    csp.AllowImages
        .FromSelf()
        .From("data:");
    csp.AllowFonts.FromSelf().From("https://fonts.googleapis.com").From("https://fonts.gstatic.com/s/");
    csp.AllowStyles
        .FromSelf()
        .From("data:")
        .From("https://fonts.googleapis.com")
        .AddNonce()
        .AllowUnsafeInline(); //Fallback for browsers that don't support nonce
    csp.AllowFrames
        .FromNowhere();
    csp.AllowPlugins
        .FromNowhere();
    csp.AllowFraming
        .FromNowhere();
});

Log.Warning("Starting up the application");
Log.CloseAndFlush();

app.UseFormHelper();
app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions {
    OnPrepareResponse = ctx => { ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=2592000"); }
});

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

app.Run();

