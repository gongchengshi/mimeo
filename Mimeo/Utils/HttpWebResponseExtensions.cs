using System;
using System.Net;
using System.Text;

namespace Mimeo.Utils
{
   public static class HttpWebResponseExtensions
   {
      public static Encoding GetEncoding(this HttpWebResponse @this)
      {
         Encoding charEncoding;
         try
         {
            charEncoding = Encoding.GetEncoding(@this.CharacterSet);
         }
         catch (ArgumentException)
         {
            charEncoding = Encoding.UTF8;
         }
         return charEncoding;
      }
   }
}
