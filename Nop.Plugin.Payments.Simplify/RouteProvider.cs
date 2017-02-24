using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.Simplify
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //HostedPaymentRedirect - redirect to hosted payment form
            routes.MapRoute("Plugin.Payments.Simplify.HostedPaymentRedirect",
                 "Plugins/PaymentSimplify/HostedPaymentRedirect",
                 new { controller = "Simplify", action = "HostedPaymentRedirect" },
                 new[] { "Nop.Plugin.Payments.Simplify.Controllers" }
            );

            //PostHostedPaymentHandler - return url of hosted payment
            routes.MapRoute("Plugin.Payments.Simplify.PostHostedPaymentHandler",
                 "Plugins/PaymentSimplify/PostHostedPaymentHandler",
                 new { controller = "Simplify", action = "PostHostedPaymentHandler" },
                 new[] { "Nop.Plugin.Payments.Simplify.Controllers" }
            );
        }

        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
