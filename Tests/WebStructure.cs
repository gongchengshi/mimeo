using System.Collections.Generic;

namespace Mimeo.Tests
{
   class WebStructure
   {
      private static string _mimeoHost = "http://localhost:63584/";

      public readonly Dictionary<string, ICollection<string>> Structure = 
         new Dictionary<string, ICollection<string>>
      {
         {
            "site1/", new List<string>
            {
               "/bad-path/../",

            }
         },
         {
            "site1/file1.html", new List<string>
            {
               "/bad-path/../file1.html"
            }
         },
         {
            "site1/file2.html", new List<string>
            {
               ""
            }
         },
         {
            "site1/path1/", new List<string>
            {
               ""
            }
         },
         {
            "site1/path1/file1.html", new List<string>
            {
               ""
            }
         },
         {
            "site1/path1/file2.html", new List<string>
            {
               ""
            }
         },
         {
            "site1/path2/", new List<string>
            {
               ""
            }
         },
         {
            "site1/path2/file1.html", new List<string>
            {
               ""
            }
         },
         {
            "site1/path2/file2.html", new List<string>
            {
               ""
            }
         }
      };

      private void _addLinks()
      {
         // Navigating to mimeo site using an absolute URL from a mimeo site with the same job
         foreach (var keyValue in Structure)
         {
            keyValue.Value.Add(_mimeoHost + "job1/site1/file1.html");
         }
      }

      public WebStructure()
      {
         _addLinks();
      }
   }
}
