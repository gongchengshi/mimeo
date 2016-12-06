using System.Web.Mvc;
using System.Web.Routing;

namespace Mimeo
{
   public class RouteConfig
   {
      public static void RegisterRoutes(RouteCollection routes)
      {
         routes.IgnoreRoute("elmah.axd");
         routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

//         routes.MapRoute(
//             name: "Home",
//             url: "",
//             defaults: new { controller = "Home", action = "Index"}
//         );
//         
//         routes.MapRoute(
//             name: "HomeIndex",
//             url: "Index.cshtml",
//             defaults: new { controller = "Home", action = "Index"}
//         );

         routes.MapRoute(
             name: "Browse",
             url: "{*path}",
             defaults: new { controller = "Browse", action = "Index" }
         );

//         routes.MapRoute(
//             name: "Default",
//             url: "{controller}/{action}/{id}",
//             defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
//         );
      }
   }
}