using Amazon.S3;
using FileManager.Application.Abstractions;
using FileManager.Application.Behaviors;
using FileManager.Application.Features.Commands.Applications;
using FileManager.Domain.Repositories;
using FileManager.Domain.Services;
using FileManager.Infrastructure.Configuration;
using FileManager.Infrastructure.Persistence.SqlServer.Connection;
using FileManager.Infrastructure.Persistence.SqlServer.Migrations;
using FileManager.Infrastructure.Persistence.SqlServer.Repositories;
using FileManager.Infrastructure.Storage;
using FileManager.Infrastructure.Storage.Abstractions;
using FileManager.Infrastructure.Storage.RustFs;
using Kootam.Cqrs.Abstractions.Behaviors;
using Kootam.Cqrs.DependencyInjections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddFileManagerInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SqlServerOptions>(configuration.GetSection(SqlServerOptions.SectionName));
        services.Configure<RustFsOptions>(configuration.GetSection(RustFsOptions.SectionName));

        AddPersistence(services);
        AddObjectStorage(services, configuration);
        AddDomainServices(services);
        AddCqrsPipeline(services);

        return services;
    }

    private static void AddPersistence(IServiceCollection services)
    {
        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
        services.AddSingleton<SqlMigrationRunner>();

        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IFolderRepository, FolderRepository>();
        services.AddScoped<IStorageFileRepository, StorageFileRepository>();
        services.AddScoped<ITrashRepository, TrashRepository>();
    }

    private static void AddObjectStorage(IServiceCollection services, IConfiguration configuration)
    {
        var rustFs = configuration.GetSection(RustFsOptions.SectionName).Get<RustFsOptions>() ?? new RustFsOptions();

        services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(
            rustFs.AccessKey,
            rustFs.SecretKey,
            new AmazonS3Config
            {
                ServiceURL = rustFs.ServiceUrl,
                ForcePathStyle = rustFs.ForcePathStyle
            }));

        // Swap this single registration to target a different provider
        // (MinIO/AWS S3/Azure Blob/local disk) without touching any calling code.
        services.AddSingleton<IObjectStorage, RustFsObjectStorage>();
        services.AddScoped<IFileStorageService, FileStorageService>();
    }

    private static void AddDomainServices(IServiceCollection services)
    {
        services.AddSingleton<IHashCalculator, HashCalculator>();
        services.AddSingleton<IFileNameGenerator, FileNameGenerator>();
        services.AddSingleton<IStoragePathGenerator, StoragePathGenerator>();
    }

    private static void AddCqrsPipeline(IServiceCollection services)
    {
        services.AddCqrs(options =>
        {
            options.RegisterServicesFromAssemblyContaining<RegisterApplicationCommand>();
        });

        // Bridges FileManager.Domain.Exceptions.DomainException (thrown by aggregates) into
        // a Result failure - Kootam.Cqrs's own DomainExceptionBehavior only catches its own
        // Kootam.Cqrs.Exceptions.DomainStateException, a different type. See the behavior's
        // XML doc for details.
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(DomainExceptionMappingBehavior<,>));
    }
}
