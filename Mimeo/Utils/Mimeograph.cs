using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Gongchengshi;

namespace Mimeo.Utils
{
   /// <summary>
   /// Take all URLs and rewrite them to be absolute URLs to their Mimeo equivalents.
   /// </summary>
   static public class Mimeograph
   {
      private static readonly Dictionary<string, List<KeyValuePair<Regex, string>>> _replacements = 
         new Dictionary<string, List<KeyValuePair<Regex, string>>>();

      public static void Initialize(IDictionary<string, HashSet<string>> domains)
      {
         foreach (var jobNameDomain in domains)
         {
            var replacementsList = new List<KeyValuePair<Regex, string>>();
            _replacements[jobNameDomain.Key] = replacementsList;

            replacementsList.AddRange(jobNameDomain.Value.Select(
               domain => new KeyValuePair<Regex, string>(
                  new Regex("https?(://|&#x3a;&#x2f;&#x2f;)" + domain),
                  "/" + jobNameDomain.Key + "/" + domain)));
         }
      }

      private static readonly string[] _replaceableApplicationMimiTypes = { "xhtml", "javascript", "atom", "rss", "rdf", "ecmascript", "soap", "dtd" };
      private static readonly string[] _replaceableTextMimiTypes = { "css", "html", "javascript" };

      public static bool IsContentTypeWithReplaceableLinks(string contentType)
      {
         return ((contentType.StartsWith("text/") && 
                  _replaceableTextMimiTypes.Any(contentType.Substring("text/".Length).Contains)) ||
                ((contentType.StartsWith("application/") && 
                  _replaceableApplicationMimiTypes.Any(contentType.Substring("application/".Length).Contains))));
      }

      public static string Execute(Stream input, string jobName)
      {
         using (var reader = new StreamReader(input))
         {
            return reader.ReadToEnd().ReplaceAll(_replacements[jobName]);
         }
      }
   }
}
