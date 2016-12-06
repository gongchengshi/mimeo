using System.Net;
using System.Web.Mvc;
using Mimeo.Utils;
using Gongchengshi.Mvc;

namespace Mimeo.Controllers
{
   public class BrowseController : Controller
   {
      public ActionResult Index()
      {
         HttpWebResponse response;
         string jobName;

         try
         {
            // Todo: Put up a warning that the user is leaving the sandbox.
            if (Request.UrlReferrer == null && Request.Url.PathAndQuery == "/")
            {
               return Content("This is Mimeo");
            }

            var address = MimeoProxy.ResolveRepoAddress(Request.UrlReferrer, Request.Url);
            //throw new PrintMessageToClient(address.ToString());
            jobName = address.Item1;

            // Get the file from S3
            response = MimeoProxy.FetchFromRepo(address.Item2.AbsoluteUri, address.Item3);
         }
         catch (MimeoNotFound ex)
         {
            return new HttpNotFoundResult(ex.Message);
         }
         catch (MimeoRedirect ex)
         {
            return new RedirectResult(ex.Destination);
         }
         catch (PrintMessageToClient ex)
         {
            return Content(ex.Message);
         }

         // Cleanse the links if the file is a type that contains links.
         // Todo: What about non-plain-text files that contain links (Word, PDF, etc...)?
         if (Mimeograph.IsContentTypeWithReplaceableLinks(response.ContentType))
         {
            using (var ds = HttpWebResponseResult.GetContentStream(response))
            {
               var contentResult = new ContentResult
               {
                  // Clean the links so that the user can't navigate from a Mimeo site to an actual site.
                  Content = Mimeograph.Execute(ds, jobName),
                  ContentType = response.ContentType
               };

               return contentResult;
            }
         }

         // Otherwise just return the file
         return new HttpWebResponseResult(response);
      }
   }
}
