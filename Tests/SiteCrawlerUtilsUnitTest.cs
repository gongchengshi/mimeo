using System;
using Amazon;
using Amazon.SimpleDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimeo.Utils;

namespace Mimeo.Tests
{
   [TestClass]
   public class SiteCrawlerUtilsUnitTest
   {
      [TestMethod]
      public void GetRedirectEndpointTest()
      {
         var sdb = new AmazonSimpleDBClient("AKIAIEWGSYRFXIUJ7KTQ", "NkBdmwIC2cg0959QN4j+xYrlNgrqWPETKSquLDrV", RegionEndpoint.USWest2);
         var job = new CrawlJob("sel11122014");

         var input = new AtraxUrl("dummy");
         var result = SiteCrawlerUtils.GetRedirectEndpoint(sdb, job, input);
         Assert.AreEqual(input.ToString(), result.ToString());

         input = new AtraxUrl("https://www.example2.com/video/");
         result = SiteCrawlerUtils.GetRedirectEndpoint(sdb, job, input);
         Assert.AreEqual("video.example2.com/", result.ToString());

         input = new AtraxUrl("https://video.example2.com/");
         result = SiteCrawlerUtils.GetRedirectEndpoint(sdb, job, input);
         Assert.AreEqual(input.ToString(), result.ToString());
      }

      [TestMethod]
      public void EncodeS3KeyTest()
      {
         var testCases = new[]
         {
            Tuple.Create(
               "www.example2.com/assets/0/114/234/236/f9895377-e729-4242-bf6e-2cf76fdcdf58.pdf#page%3D2",
               "www.example2.com/assets/0/114/234/236/f9895377-e729-4242-bf6e-2cf76fdcdf58.pdf%23page%253D2"),
            Tuple.Create(
               "www.example2.com/DynamicTabs.aspx?id=21474836964",
               "www.example2.com/DynamicTabs.aspx%3Fid%3D21474836964")
         };

         foreach (var testCase in testCases)
         {
            var actual = SiteCrawlerUtils.EncodeS3Key(testCase.Item1);
            Assert.AreEqual(testCase.Item2, actual);
         }
      }
   }
}
