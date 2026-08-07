using FileManager.Infrastructure.Configuration;
using FileManager.Infrastructure.DependencyInjection;
using FileManager.Infrastructure.Persistence.SqlServer.Migrations;
using FileManager.Infrastructure.Seeding;
using Kootam.Translator.Database.DependencyInjection;
using Kootam.Utilities.ScalarRegistration.DependencyInjection;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.AddFileManagerTranslator();
builder.Services.AddFileManagerInfrastructure(builder.Configuration);

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var defaultCulture = builder.Configuration["Translator:DefaultCulture"] ?? "fa-IR";

    options.SetDefaultCulture(defaultCulture);
    options.AddSupportedCultures("en-US", "fa-IR");
    options.AddSupportedUICultures("en-US", "fa-IR");
    options.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());
});

builder.Services.AddScalar(options =>
{
    options.Enabled = true;
    options.Description = "The FileManager project for Scalar";
    options.Name = "FileManager Project";
    options.Version = "1.0.0";
});

var app = builder.Build();

var sqlServerOptions = app.Services.GetRequiredService<IOptions<SqlServerOptions>>().Value;
if (sqlServerOptions.AutoMigrate)
{
    using var scope = app.Services.CreateScope();
    var migrator = scope.ServiceProvider.GetRequiredService<SqlMigrationRunner>();
    await migrator.RunAsync();

    var userSeeder = scope.ServiceProvider.GetRequiredService<UserSeeder>();
    await userSeeder.SeedAsync();
}

app.UseTranslator();

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseRequestLocalization();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseScalar();

app.Run();
