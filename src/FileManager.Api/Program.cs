using FileManager.Infrastructure.DependencyInjection;
using FileManager.Infrastructure.Persistence.SqlServer.Migrations;
using Kootam.Utilities.ScalarRegistration.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddFileManagerInfrastructure(builder.Configuration);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScalar(options =>
{
    options.Enabled = true;
    options.Description = "The FileManager project for Scalar";
    options.Name = "FileManager Project";
    options.Version = "1.0.0";
});
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var migrator = scope.ServiceProvider.GetRequiredService<SqlMigrationRunner>();
    await migrator.RunAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseScalar();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();