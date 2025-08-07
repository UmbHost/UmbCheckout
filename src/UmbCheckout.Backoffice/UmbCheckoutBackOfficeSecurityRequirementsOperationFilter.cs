using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using UmbCheckout.Shared;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace UmbCheckout.Backoffice
{
    public class UmbCheckoutBackOfficeSecurityRequirementsOperationFilter : BackOfficeSecurityRequirementsOperationFilterBase
    {
        protected override string ApiName => Consts.ApiName;
    }


    public class UmbCheckoutConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
    {
        public void Configure(SwaggerGenOptions options)
        {
            options.SwaggerDoc(Consts.ApiName, new OpenApiInfo { Title = Consts.ApiTitle, Version = Consts.ApiVersion });
            options.OperationFilter<UmbCheckoutBackOfficeSecurityRequirementsOperationFilter>();
        }
    }

    public class UmbCheckoutSwaggerComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<IOperationIdHandler, UmbCheckoutCustomOperationHandler>();
            builder.Services.ConfigureOptions<UmbCheckoutConfigureSwaggerGenOptions>();
        }
    }
}
