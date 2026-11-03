using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Core.DependencyInjection;

namespace UmbCheckout.Core.ManagementApi
{
    public static class UmbCheckoutApiDocumentExtensions
    {
        /// <summary>
        /// Registers a dedicated swagger document for a package (Umbraco "Adding a custom Swagger document"),
        /// with back-office security and readable operation/schema IDs.
        /// </summary>
        public static IUmbracoBuilder AddUmbCheckoutApiDocument<TSecurityFilter>(
            this IUmbracoBuilder builder,
            string documentName,
            string title,
            string namespacePrefix,
            string? excludedNamespacePrefix = null)
            where TSecurityFilter : BackOfficeSecurityRequirementsOperationFilterBase
        {
            builder.Services.Configure<SwaggerGenOptions>(options =>
            {
                options.SwaggerDoc(documentName, new OpenApiInfo { Title = title, Version = "1.0" });
                options.OperationFilter<TSecurityFilter>();
            });

            builder.Services.AddSingleton<IOperationIdHandler>(sp =>
                new UmbCheckoutOperationIdHandler(sp.GetRequiredService<IOptions<ApiVersioningOptions>>(), namespacePrefix, excludedNamespacePrefix));
            builder.Services.AddSingleton<ISchemaIdHandler>(_ => new UmbCheckoutSchemaIdHandler(namespacePrefix, excludedNamespacePrefix));

            return builder;
        }
    }
}
