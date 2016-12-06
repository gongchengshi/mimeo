using System;
using System.Net;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Mimeo
{
   public class MvcApplication : System.Web.HttpApplication
   {
      protected void Application_Start()
      {
         AreaRegistration.RegisterAllAreas();

         FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
         RouteConfig.RegisterRoutes(RouteTable.Routes);
         BundleConfig.RegisterBundles(BundleTable.Bundles);
      }
   }

   public static class Globals
   {
#if DEBUG
      public const string HostName = "localhost:63584";
      public const string HostAddress = "http://" + HostName;
      public static readonly Uri HostUri = new Uri(HostAddress);
#else
      public const string HostName = "";
      public const string HostAddress = "https://" + HostName;
#endif
   }
}