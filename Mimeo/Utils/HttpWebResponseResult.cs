using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Web.Mvc;

namespace Gongchengshi.Mvc
{
   /// <summary>
   /// Result for relaying an HttpWebResponse
   /// Acquired from: http://www.wiliam.com.au/wiliam-blog/web-design-sydney-relaying-an-httpwebresponse-in-asp-net-mvc
   /// </summary>
   public class HttpWebResponseResult : ActionResult
   {
      private readonly HttpWebResponse _response;
      private readonly ActionResult _innerResult;

      public static Stream GetContentStream(HttpWebResponse response)
      {
         Stream contentStream;
         if (response.ContentEncoding.Contains("gzip"))
         {
            contentStream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);
         }
         else if (response.ContentEncoding.Contains("deflate"))
         {
            // According to this the zlib compression is not the same as deflate used in .NET
            // See: http://george.chiramattel.com/blog/2007/09/deflatestream-block-length-does-not-match.html
            // Atrax used zlib and called it deflate so this is here as a work around. Atrax switched to using gzip in the future.
            var s = response.GetResponseStream();
            // Most browsers will handle all three compression formats.
            // Todo: Detect if using zlib compression first before stripping the zlib header bytes
            s.ReadByte();
            s.ReadByte();
            contentStream = new DeflateStream(s, CompressionMode.Decompress);
         }
         else
         {
            contentStream = response.GetResponseStream();
         }
         return contentStream;
      }

      /// <summary>
      /// Relays an HttpWebResponse as verbatim as possible.
      /// </summary>
      /// <param name="responseToRelay">The HTTP response to relay</param>
      public HttpWebResponseResult(HttpWebResponse responseToRelay)
      {
         if (responseToRelay == null)
         {
            throw new ArgumentNullException("responseToRelay");
         }

         _response = responseToRelay;

         var contentStream = GetContentStream(responseToRelay);

         if (string.IsNullOrEmpty(responseToRelay.CharacterSet))
         {
            // File result
            _innerResult = new FileStreamResult(contentStream, responseToRelay.ContentType);
         }
         else
         {
            // Text result
            _innerResult = new ContentResult { Content = new StreamReader(contentStream).ReadToEnd() };
         }
      }

      /// <summary>
      /// Todo: Is it possible to just pass the data through and set the Content-Encoding header 
      /// to response.ContentEncoding instead of decompressing?
      /// </summary>
      /// <param name="context"></param>
      public override void ExecuteResult(ControllerContext context)
      {
         var clientResponse = context.HttpContext.Response;
         clientResponse.StatusCode = (int)_response.StatusCode;

         foreach (var headerKey in _response.Headers.AllKeys)
         {
            switch (headerKey)
            {
               case "Content-Length":
               case "Transfer-Encoding":
               case "Content-Encoding":
                  // Handled by IIS
                  break;

               default:
                  clientResponse.AddHeader(headerKey, _response.Headers[headerKey]);
                  break;
            }
         }

         _innerResult.ExecuteResult(context);
      }
   }
}
