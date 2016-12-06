using System.Collections.Generic;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Mimeo.Tests.MimeoProxyTestCase;
using Gongchengshi.Collections.Generic;

namespace Mimeo.Tests
{
    static class TestBucketBuilder
   {
      private const string _fileContentsFormat = @"
<html>
<head>
<title>{0}</title>
</head>
<body>
{1}
</body>
</html>
";
      private const string _linkFormat = "<a href=\"{0}\" job=\"{1}\" expected=\"{2}\" redirect=\"{3}\">{0}</a>";

      private const string _policyFormat = @"
{{
	""Version"": ""2012-10-17"",
	""Statement"": [
		{{
			""Sid"": ""PublicReadGetObject"",
			""Effect"": ""Allow"",
			""Principal"": ""*"",
			""Action"": ""s3:GetObject"",
			""Resource"": ""arn:aws:s3:::{0}/*""
		}},
		{{
			""Sid"": ""PublicListObjects"",
			""Effect"": ""Allow"",
			""Principal"": ""*"",
			""Action"": ""s3:ListBucket"",
			""Resource"": ""arn:aws:s3:::{0}""
		}}
	]
}}
";

      private static string _mimeoHost = "localhost:63584";

      static public void BuildBuckets(IList<string> paths, IEnumerable<TestCase> testCases)
      {
         var s3 = new AmazonS3Client(RegionEndpoint.USWest2);

         var bucketNames = new[] { "job1.content"/*, "job2.content" */};

         foreach (var bucketName in bucketNames)
         {
            try
            {
               s3.PutBucket(bucketName);
               s3.PutBucketPolicy(bucketName, string.Format(_policyFormat, bucketName));
            }
            catch (AmazonS3Exception ex)
            {
               if (ex.ErrorCode != "BucketAlreadyOwnedByYou")
               {
                  throw;
               }
            }

            var referrers = new DictionaryOfLists<string, TestCase>();

            foreach (var testCase in testCases)
            {
               var url = testCase.Input.Referrer.AbsoluteUri;
               var key = url.Substring("http://mimeo/job1/".Length);
               referrers.Add(key.TrimEnd('/'), testCase);
            }

            foreach (var path in paths)
            {
               var key = path.TrimEnd('/');
               var links = referrers[key];
               string linksHtml = string.Empty;

               foreach (var testCase in links)
               {
                  linksHtml += "\n<br />" + string.Format(_linkFormat, testCase.Input.RawUrl.Replace("mimeo", _mimeoHost), 
                     testCase.Output.JobName, testCase.Output.Url, testCase.Output.RequiresRedirect);
               }

               var contents = string.Format(_fileContentsFormat, path, linksHtml);

               s3.PutObject(new PutObjectRequest { BucketName = bucketName, ContentBody = contents, Key = key, ContentType = "text/html"});
            }
         }
      }
   }
}
