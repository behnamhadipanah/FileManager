using FileManager.Infrastructure.Configuration;
using FileManager.Infrastructure.DependencyInjection;
using FileManager.Infrastructure.Persistence.SqlServer.Migrations;
using FileManager.Infrastructure.Seeding;
using Kootam.Utilities.ScalarRegistration.DependencyInjection;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddFileManagerInfrastructure(builder.Configuration);

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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseScalar();
}

app.Run();
