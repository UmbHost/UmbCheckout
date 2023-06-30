using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Moq;

namespace UmbCheckout.Tests.Helpers
{
    internal class HttpContextAccessor
    {
        public static Microsoft.AspNetCore.Http.HttpContextAccessor GetHttpContext(string incomingRequestUrl, string host)
        {
            var context = new DefaultHttpContext();
            var session = new Mock<ISession>();
            context.Request.Path = incomingRequestUrl;
            context.Request.Host = new HostString(host);
            context.Session = session.Object;
            //Do your thing here...

            var obj = new Microsoft.AspNetCore.Http.HttpContextAccessor();
            obj.HttpContext = context;
            return obj;
        }
    }
}
