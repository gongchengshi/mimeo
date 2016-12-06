using System.Web.Mvc;

namespace Mimeo.Controllers
{
   public class HomeController : Controller
   {
      public ActionResult Index()
      {
         return View();
//         return View(Globals.Jobs);
      }
   }
}
