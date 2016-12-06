using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimeo.Utils;
using Mimeo.Tests.MimeoProxyTestCase;

namespace Mimeo.Tests
{
   [TestClass]
   public class MimeoProxyTranslateAddressUnitTest
   {
      private static readonly List<string> _webStructure = BuildWebStucture();

      private static List<string> BuildWebStucture()
      {
         var formatStrings = new[]
         {
            "site1/",
            "site1/file1.html",
            "site1/file2.html",
            "site1/path1/",
            "site1/path1/file1.html",
            "site1/path1/file2.html",
            "site1/path2/",
            "site1/path2/file1.html",
            "site1/path2/file2.html"
         };

         var webStructure = new List<string>();

         foreach (var path in formatStrings)
         {
            webStructure.Add(path);
            webStructure.Add(path + "?param1=value1");
         }

         return webStructure;
      }

      private static IEnumerable<TestCase> BuildExternalReferrerTestCases()
      {
         var testCases = new List<TestCase>();

         foreach (var address in _webStructure)
         {
            testCases.Add(new TestCase("Navigating directly (using an absolute url) to mimeo site without a referrer",
               new Input(null, "http://mimeo/job1/" + address),
               new Output("job1", address, false)));
         }

         foreach (var address in _webStructure)
         {
            testCases.Add(new TestCase("Navigating to mimeo site using an absolute URL from a non-mimeo site",
               new Input("http://not-mimeo/", "http://mimeo/job1/" + address),
               new Output("job1", address, false)));
         }

         foreach (var address in _webStructure)
         {
            testCases.Add(new TestCase("Navigating to mimeo site using an absolute URL from a mimeo site with a different job",
               new Input("http://mimeo/job1/site1/", "http://mimeo/job2/" + address),
               new Output("job2", address, false)));
         }

         return testCases;
      }

      [TestMethod]
      public void ExternalReferrerTest()
      {
         foreach (var testCase in BuildExternalReferrerTestCases())
         {
            var actual = MimeoProxy.TranslateAddress(testCase.Input.Referrer, new Uri(testCase.Input.RawUrl));
            Assert.AreEqual(testCase.Output.JobName, actual.Item1);
            Assert.AreEqual(testCase.Output.Url, actual.Item2.ToString());
            Assert.AreEqual(testCase.Output.RequiresRedirect, actual.Item3);
         }
      }

      private static IEnumerable<TestCase> BuildInternalReferrerTestCases()
      {
         var testCases = new List<TestCase>();

         testCases.Add(new TestCase("Navigating to mimeo site using an absolute URL from a mimeo site with the same job",
            new Input("http://mimeo/job1/site1/", "http://mimeo/job1/site1/file1.html"),
            new Output("job1", "site1/file1.html", false)));

         var generalCaseName = "Navigating within site using root-relative url";
         foreach (var address in _webStructure)
         {
            testCases.Add(new TestCase(generalCaseName,
               new Input("http://mimeo/job1/site1/", address.Substring("site1".Length)),
               new Output("job1", address, true)));
         }

         testCases.Add(new TestCase(generalCaseName,
               new Input("http://mimeo/job1/site1/", "/bad-path/../"),
               new Output("job1", "site1/", true)));

         testCases.Add(new TestCase(generalCaseName,
               new Input("http://mimeo/job1/site1/", "/bad-path/../file1.html"),
               new Output("job1", "site1/file1.html", true)));

         testCases.Add(new TestCase(generalCaseName,
               new Input("http://mimeo/job1/site1/", "/bad-path/../../file1.html"),
               new Output("job1", "site1/file1.html", true)));

         testCases.Add(new TestCase(generalCaseName,
               new Input("http://mimeo/job1/site1/", "/../file1.html"),
               new Output("job1", "site1/file1.html", true)));

         testCases.Add(new TestCase(generalCaseName,
               new Input("http://mimeo/job1/site1/", "/../../file1.html"),
               new Output("job1", "site1/file1.html", true)));

         testCases.Add(new TestCase(generalCaseName,
               new Input("http://mimeo/job1/site1/", "/.././file1.html"),
               new Output("job1", "site1/file1.html", true)));

         testCases.Add(new TestCase(generalCaseName,
               new Input("http://mimeo/job1/site1/", "/../bad-path/file1.html"),
               new Output("job1", "site1/bad-path/file1.html", true)));

         generalCaseName = "Navigating from site root using current-directory-relative URL";
         var currentRelativeLinks = new List<string>
         {
            "file1.html",
            "?param1=value1",
            "path1/file1.html",
         };

         foreach (var path in currentRelativeLinks)
         {
            testCases.Add(new TestCase(generalCaseName,
                           new Input("http://mimeo/job1/site1/", path),
                           new Output("job1", "site1/" + path, true)));
         }

         generalCaseName = "Navigating within site using current-directory-relative URL";
         testCases.Add(new TestCase(generalCaseName,
               new Input("http://mimeo/job1/site1/", "../?param1=value1"),
               new Output("job1", "site1/?param1=value1", true)));

         testCases.Add(new TestCase(generalCaseName,
               new Input("http://mimeo/job1/site1/", "../file2.html"),
               new Output("job1", "site1/file2.html", true)));

         generalCaseName = "Navigating within site using current-directory-relative URL";
         testCases.Add(new TestCase(generalCaseName,
               new Input("http://mimeo/job1/site1/path1/", "file1.html"),
               new Output("job1", "site1/path1/file1.html", true)));

         testCases.Add(new TestCase(generalCaseName,
               new Input("http://mimeo/job1/site1/path1/file1.html", "file2.html"),
               new Output("job1", "site1/path1/file2.html", true)));

         testCases.Add(new TestCase(generalCaseName,
               new Input("http://mimeo/job1/site1/path1/file1.html", "?param1=value1"),
               new Output("job1", "site1/path1/file1.html?param1=value1", true)));

         testCases.Add(new TestCase(generalCaseName,
               new Input("http://mimeo/job1/site1/path1/file1.html", "../?param1=value1"),
               new Output("job1", "site1/?param1=value1", true)));

         testCases.Add(new TestCase(generalCaseName,
               new Input("http://mimeo/job1/site1/path1/file1.html", "../file2.html"),
               new Output("job1", "site1/file2.html", true)));

         testCases.Add(new TestCase(generalCaseName,
               new Input("http://mimeo/job1/site1/path1/file1.html", "../path2/file1.html"),
               new Output("job1", "site1/path2/file1.html", true)));

         testCases.Add(new TestCase(generalCaseName,
               new Input("http://mimeo/job1/site1/path1/file1.html", "../../path2/file1.html"),
               new Output("job1", "site1/path2/file1.html", true)));

         testCases.Add(new TestCase(generalCaseName,
               new Input("http://mimeo/job1/site1/path1/file1.html", "../path2/../file1.html"),
               new Output("job1", "site1/file1.html", true)));

         return testCases;
      }

      [TestMethod]
      public void InternalReferrerTest()
      {
         var testCases = BuildInternalReferrerTestCases();

         foreach (var testCase in testCases)
         {
            var url = new Uri(testCase.Input.Referrer, testCase.Input.RawUrl);
            var actual = MimeoProxy.TranslateAddress(testCase.Input.Referrer, url);
            Assert.AreEqual(testCase.Output.JobName, actual.Item1);
            Assert.AreEqual(testCase.Output.Url, actual.Item2.ToString());
            var fullUrl = new Uri(url, "/" + testCase.Output.JobName + "/" + testCase.Output.Url);
            Assert.AreEqual(fullUrl != url, actual.Item3);
         }
      }

      [TestMethod]
      public void InternalReferrerSandboxTest()
      {
         var testCases = BuildInternalReferrerTestCases();
         TestBucketBuilder.BuildBuckets(_webStructure, testCases);
//
//         foreach (var testCase in testCases)
//         {
//            using (var browser = new IE())
//            {
//
//            }
//         }
      }
   }
}
