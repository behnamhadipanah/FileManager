using FileManager.Infrastructure.Configuration;
using FileManager.Infrastructure.Localization;
using Kootam.Translator.Database.DependencyInjection;
using Kootam.Translator.Database.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace FileManager.Infrastructure.DependencyInjection;

public static class TranslatorServiceCollectionExtensions
{
    public static WebApplicationBuilder AddFileManagerTranslator(this WebApplicationBuilder builder)
    {
        builder.AddDbTranslator(options =>
        {
            builder.Configuration.GetSection(TranslatorOptions.DefaultTranslatorOptionsName).Bind(options);

            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                options.ConnectionString = builder.Configuration
                    .GetSection(SqlServerOptions.SectionName)
                    .GetValue<string>(nameof(SqlServerOptions.ConnectionString)) ?? string.Empty;
            }

            if (options.DefaultTranslations.Length == 0)
                options.DefaultTranslations = DomainMessageDefaults.Create();
        })
        .UseCaching(reloadIntervalInMinutes: 5);

        return builder;
    }
}
