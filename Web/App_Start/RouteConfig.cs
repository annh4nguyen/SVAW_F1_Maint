using System.Web.Mvc;
using System.Web.Routing;

namespace avSVAW
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
               name: "Order",
               url: "{controller}/{action}/{id}/{ordertype}",
               defaults: new { controller = "Order", id = UrlParameter.Optional, ordertype = UrlParameter.Optional }
           );


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Login", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
