using UmbCheckout.Core.ManagementApi;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace UmbCheckout.Backoffice
{
    public class UmbCheckoutApiComposer : IComposer
    {
        public const string DocumentName = "umbcheckout";

        public void Compose(IUmbracoBuilder builder)
            => builder.AddUmbCheckoutApiDocument<UmbCheckoutSecurityFilter>(DocumentName, "UmbCheckout API", "UmbCheckout.Backoffice");

        public class UmbCheckoutSecurityFilter : BackOfficeSecurityRequirementsOperationFilterBase
        {
            protected override string ApiName => DocumentName;
        }
    }
}
