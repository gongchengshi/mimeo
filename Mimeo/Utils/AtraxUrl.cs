using System;
using Gongchengshi;

namespace Mimeo.Utils
{
   public class AtraxUrl : SchemalessUrl
   {
      public AtraxUrl(Uri uri) : base(uri)
      {}

      public AtraxUrl(string url) : base(url)
      {}

      public AtraxUrl(SchemalessUrl url) : base(url)
      {}

      public AtraxUrl Canonize()
      {
         return new AtraxUrl(SortQueryString());
      }

      public string S3Key { get { return Url.TrimEnd('/', '?'); } }
   }
}
