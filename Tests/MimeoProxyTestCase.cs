using System;

namespace Mimeo.Tests.MimeoProxyTestCase
{
   public class Input
   {
      public Input(string referrer, string rawUrl)
      {
         Referrer = referrer == null ? null : new Uri(referrer);
         RawUrl = rawUrl;
      }

      public Uri Referrer;
      public string RawUrl;
   }

   public class Output
   {
      public Output(string jobName, string url, bool requiresRedirect)
      {
         JobName = jobName;
         Url = url;
         RequiresRedirect = requiresRedirect;
      }

      public string JobName;
      public string Url;
      public bool RequiresRedirect;
   }

   public struct TestCase
   {
      public TestCase(string name, Input input, Output output)
      {
         Name = name;
         Input = input;
         Output = output;
      }

      public TestCase(string name,
         string referrer, string rawUrl,
         string jobName, string outputUrl, bool requiresRedirect)
      {
         Name = name;
         Input = new Input(referrer, rawUrl);
         Output = new Output(jobName, outputUrl, requiresRedirect);
      }

      public string Name;
      public Input Input;
      public Output Output;
   }
}
