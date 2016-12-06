using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimeo.Tests.MimeoProxyTestCase;
using Mimeo.Utils;

namespace Mimeo.Tests
{
   [TestClass]
   public class MimeoProxyResolveAddressUnitTest
   {
      private static readonly IList<TestCase> _testCases = BuildTestCases();
      private static readonly Dictionary<string, List<string>> _webStructure = BuildWebStucture();

      private static Dictionary<string, List<string>> BuildWebStucture()
      {
         var formatStrings = new[]
         {
            "mimeo/job{0}/site{1}/",
            "mimeo/job{0}/site{1}/file1.html",
            "mimeo/job{0}/site{1}/file2.html",
            "mimeo/job{0}/site{1}/path1/",
            "mimeo/job{0}/site{1}/path1/file1.html",
            "mimeo/job{0}/site{1}/path1/file2.html",
            "mimeo/job{0}/site{1}/path2/",
            "mimeo/job{0}/site{1}/path2/file1.html",
            "mimeo/job{0}/site{1}/path2/file2.html"
         };

         var jobs = new[]
         {
            new[] {"1", "2"},
            new[] {"1", "3"}
         };

         var webStructure = new Dictionary<string, List<string>>();

         foreach (var pathFormat in formatStrings)
         {
            for (int job = 1; job <= jobs.Length; ++job)
            {
               var jobId = job.ToString();
               var jobWebStructure = new List<string>();
               foreach (var jobSite in jobs[job])
               {
                  var baseName = string.Format(pathFormat, jobId, jobSite);
                  jobWebStructure.Add(baseName);
                  jobWebStructure.Add(baseName + "?param1=value1");
                  jobWebStructure.Add(baseName + "#frag1");
               }

               webStructure["job" + jobId] = jobWebStructure;
            }
         }

         return webStructure;
      }

      private static List<TestCase> BuildTestCases()
      {
         var testCases = new List<TestCase>();
//         // Navigating directly (using an absolute url) to mimeo site without a referrer
//         foreach (var address in _webStructure)
//         {
//            testCases.Add(new TestCase(
//               new Input(null, "http://mimeo/job1/" + address, "http://mimeo/job1/" + address), 
//               new Output("job1", address, false)));
//         }
//
//         // Navigating to mimeo site using an absolute URL from a non-mimeo site
//         foreach (var address in _webStructure)
//         {
//            testCases.Add(new TestCase(
//               new Input("http://not-mimeo/", "http://mimeo/job1/" + address, "http://mimeo/job1/" + address), 
//               new Output("job1", address, false)));
//         }
//
//         // Navigating to mimeo site using an absolute URL from a mimeo site with the same job
//         testCases.Add(new TestCase(
//            new Input(null, "http://mimeo/job1/site1/", "http://mimeo/job1/site1/file1.html"), 
//            new Output("job1", "site1/file1.html", false)));
//
//         // Navigating to mimeo site using an absolute URL from a mimeo site with a different job
//         foreach (var address in _webStructure)
//         {
//            testCases.Add(new TestCase(
//               new Input("http://mimeo/job1/site1/", "http://mimeo/job1/" + address, "http://mimeo/job1/" + address), 
//               new Output("job2", address, false)));
//         }
//
//         // Navigating within site using root-relative url
//         foreach (var address in _webStructure)
//         {
//            testCases.Add(new TestCase(
//               new Input("http://mimeo/job1/site1/", "http://mimeo/job1/site1/" + address, address.Substring("site1".Length)),
//               new Output("job2", address, true)));
//         }
//
//         testCases.Add(new TestCase(
//               new Input("http://mimeo/job1/site1/", "http://mimeo/job1/site1/" + "bad-path/../", "/bad-path/../"),
//               new Output("job2", "http://mimeo/job1/site1/", true)));
//
//         testCases.Add(new TestCase(
//               new Input("http://mimeo/job1/site1/", "http://mimeo/job1/site1/" + "bad-path/../file1.html", "/bad-path/../file1.html"),
//               new Output("job2", "http://mimeo/job1/site1/file1.html", true)));
//
//         testCases.Add(new TestCase(
//               new Input("http://mimeo/job1/site1/", "http://mimeo/job1/site1/" + "bad-path/../../file1.html", "/bad-path/../../file1.html"),
//               new Output("job2", "http://mimeo/job1/site1/file1.html", true)));
//
//         testCases.Add(new TestCase(
//               new Input("http://mimeo/job1/site1/", "http://mimeo/job1/site1/" + "../file1.html", "/../file1.html"),
//               new Output("job2", "http://mimeo/job1/site1/file1.html", true)));
//
//         testCases.Add(new TestCase(
//               new Input("http://mimeo/job1/site1/", "http://mimeo/job1/site1/" + "../../file1.html", "/../../file1.html"),
//               new Output("job2", "http://mimeo/job1/site1/file1.html", true)));
//
//         testCases.Add(new TestCase(
//               new Input("http://mimeo/job1/site1/", "http://mimeo/job1/site1/" + ".././file1.html", "/.././file1.html"),
//               new Output("job2", "http://mimeo/job1/site1/file1.html", true)));
//
//         testCases.Add(new TestCase(
//               new Input("http://mimeo/job1/site1/", "http://mimeo/job1/site1/" + "../bad-path/file1.html", "/../bad-path/file1.html"),
//               new Output("job2", "http://mimeo/job1/site1/bad-path/file1.html", true)));

         return testCases;
      }

      private const string _jobName = "";

      private static readonly Uri _localHost = new Uri("http://localhost:63584/");
      private static readonly Uri _localPath = new Uri(_localHost, _jobName + "/");
      private readonly string _s3Host = string.Format("http://{0}.content.s3-website-us-east-1.amazonaws.com/", _jobName);

      private string _rawUrl(string path)
      {
         return "/" + _jobName + "/" + path;
      }

      #region RawURL is absolute
      /// <summary>
      /// User enters Mimeo URL directly into browser or there is no referrer information
      /// </summary>
      [TestMethod]
      public void DirectNavigation()
      {
         const string path = "www.example2.com";
         var url = new Uri(_localPath, path);
         var result = MimeoProxy.ResolveRepoAddress(null, url);
         Assert.AreEqual(_jobName, result.Item1);
         Assert.AreEqual(_s3Host + path, result.Item2);
      }

       

      [TestMethod]
      public void DirectNavigationLong()
      {
         const string path = "www.example.com/4391supported";
         var url = new Uri(_localPath, path);
         var result = MimeoProxy.ResolveRepoAddress(null, url);
         Assert.AreEqual(_jobName, result.Item1);
         Assert.AreEqual(_s3Host + path, result.Item2);
      }

      [TestMethod]
      public void DirectToRoot()
      {
         const string path = "www.example2.com/";
         var url = new Uri(_localPath, path);
         var result = MimeoProxy.ResolveRepoAddress(null, url);
         Assert.AreEqual(_jobName, result.Item1);
         Assert.AreEqual(_s3Host + path.TrimEnd('/'), result.Item2);
      }

      [TestMethod]
      public void LinkFromNonMimeoSite()
      {
         const string path = "www.example2.com";
         var referrer = new Uri("http://www.example.com/index.html");
         var url = new Uri(_localPath, path);
         var result = MimeoProxy.ResolveRepoAddress(referrer, url);
         Assert.AreEqual(_jobName, result.Item1);
         Assert.AreEqual(_s3Host + path, result.Item2);
      }

      [TestMethod]
      public void ReferralToAbsoluteUrl()
      {
         var referrer = new Uri(_localPath, "index.html");
         const string path = "www.example2.com/4391supported";
         var url = new Uri(_localPath, path);
         var result = MimeoProxy.ResolveRepoAddress(referrer, url);
         Assert.AreEqual(_jobName, result.Item1);
         Assert.AreEqual(_s3Host + path, result.Item2);
      }

      [TestMethod]
      public void SiteToSiteReferralAbsoluteUrl()
      {
         var referrer = new Uri(_localPath, "www.example2.com/61850");
         const string path = "www.example2.com/4391supported";
         var url = new Uri(_localPath, path);
         var result = MimeoProxy.ResolveRepoAddress(referrer, url);
         Assert.AreEqual(_jobName, result.Item1);
         Assert.AreEqual(_s3Host + path, result.Item2);
      }

      [TestMethod]
      public void InternalReferralAbsoluteUrl()
      {
         var referrer = new Uri(_localPath, "www.example2.com.mx/analisis.php");
         const string path = "www.example2.com/4391supported";
         var url = new Uri(_localPath, path);
         var result = MimeoProxy.ResolveRepoAddress(referrer, url);
         Assert.AreEqual(_jobName, result.Item1);
         Assert.AreEqual(_s3Host + path, result.Item2);
      }
        #endregion

        private readonly string _relativePathWithQuery;

//      #region Relative URLs
//      [TestMethod]
//      public void RelativePathQuery()
//      {
//         const string referrerDir = "www.example2.com/AutomationControllers/";
//         const string referrerPath = referrerDir + "?LangType=1034";
//         const string path = "?LangType=1033";
//         var referrer = new Uri(_localPath, referrerPath);
//         var url = new Uri(referrer, path);
//         var result = MimeoProxy.ResolveAddress(referrer, url, path);
//         Assert.AreEqual(_jobName, result.Item1);
//         var expected = _s3Host + SiteCrawlerUtils.EncodeS3Key(referrerDir + path);
//         Assert.AreEqual(expected, result.Item2);
//      }
//
//      [TestMethod]
//      public void RelativePath()
//      {
//         const string referrerDir = "www.example2.com/AutomationSystems/";
//         const string referrerPath = referrerDir + "Accessories";
//         const string path = "?LangType=1033";
//         var referrer = new Uri(_localPath, referrerPath);
//         var url = new Uri(referrer, path);
//         var result = MimeoProxy.ResolveAddress(referrer, url, path);
//         Assert.AreEqual(_jobName, result.Item1);
//         var expected = _s3Host + SiteCrawlerUtils.EncodeS3Key(referrerDir + path);
//         Assert.AreEqual(expected, result.Item2);
//      }
//
////      [TestMethod]
////      public void UpDirectoryRelativePathQuery()
////      {
////         const string referrerDir = "www.example2.com/AutomationControllers/";
////         const string referrerPath = referrerDir + "?LangType=1034";
////         const string path = "?LangType=1033";
////         var referrer = new Uri(_localPath, referrerPath);
////         var url = new Uri(referrer, path);
////         var result = MimeoProxy.ResolveAddress(referrer, url, path);
////         Assert.AreEqual(_jobName, result.Item1);
////         var expected = _s3Host + SiteCrawlerUtils.EncodeS3Key(referrerDir + path);
////         Assert.AreEqual(expected, result.Item2);
////      }
//
//      [TestMethod]
//      public void RootRelativePath()
//      {
//         const string referrerPath = "www.example2.com/";
//         const string path = "AlarmPanels";
//         var referrer = new Uri(_localPath, referrerPath);
//         var url = new Uri(_localHost, path);
//         var result = MimeoProxy.ResolveAddress(referrer, url, "/" + path);
//         Assert.AreEqual(_jobName, result.Item1);
//         var expected = _s3Host + SiteCrawlerUtils.EncodeS3Key(referrerPath + path);
//         Assert.AreEqual(expected, result.Item2);
//      }
//
//      /// <summary>
//      /// This may work in the unit test but the routing rules may send this request to IndexController
//      /// </summary>
//      [TestMethod]
//      public void Root()
//      {
//         const string referrerPath = "www.example2.com/";
//         const string path = "";
//         var referrer = new Uri(_localPath, referrerPath);
//         var url = new Uri(_localHost, path);
//         var result = MimeoProxy.ResolveAddress(referrer, url, "/" + path);
//         Assert.AreEqual(_jobName, result.Item1);
//         var expected = _s3Host + SiteCrawlerUtils.EncodeS3Key(referrerPath + path);
//         Assert.AreEqual(expected, result.Item2);
//      }
//      #endregion
   }
}
