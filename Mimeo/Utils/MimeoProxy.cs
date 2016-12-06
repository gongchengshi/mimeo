using System;
using System.Collections.Generic;
using System.Net;
using Amazon;
using Amazon.S3;
using Amazon.SimpleDB;
using Gongchengshi;

namespace Mimeo.Utils
{
   public static class MimeoProxy
   {
      private static readonly IDictionary<string, Uri> _bucketUris = new Dictionary<string, Uri>();
      private static readonly Dictionary<string, HashSet<string>> _domains = new Dictionary<string, HashSet<string>>();
      private static readonly AmazonSimpleDBClient _sdb = new AmazonSimpleDBClient(RegionEndpoint.USWest2);

      static MimeoProxy()
      {
         var s3 = new AmazonS3Client(RegionEndpoint.USWest2);
         foreach (var bucket in SiteCrawlerUtils.GetCrawledContentBuckets(s3))
         {
            try
            {
               var jobName = SiteCrawlerUtils.GetJobNameFromBucketName(bucket.BucketName);
               _domains[jobName] = new HashSet<string>(SiteCrawlerUtils.GetDomains(s3, bucket.BucketName));

               // If using the static website to serve resources
               // _bucketUris.Add(jobName,
               //   new Uri(String.Format("http://{0}.s3-website-{1}.amazonaws.com/", bucket.BucketName, bucketLocation)));
               _bucketUris.Add(jobName,
                  new Uri(String.Format("https://s3-{0}.amazonaws.com/{1}/", "us-west-2", bucket.BucketName)));
            }
            catch (AmazonS3Exception)
            {
               // The bucket must be in the US region but not us-west-2. Go figure.
            }
         }

         Mimeograph.Initialize(_domains);
      }

      /// <summary>
      /// Given the input figure out what the S3 address is for the requested resource.
      /// Url must have at least one segment in its path. This is ensured by the routing rules for BrowseController.
      /// </summary>
      /// <param name="referrer"></param>
      /// <param name="url"></param>
      /// <returns>A Tuple (job name, URL of the resource in S3)</returns>
      public static Tuple<string, AtraxUrl, bool> TranslateAddress(Uri referrer, Uri url)
      {
         string pathAndQuery = url.PathAndQuery;
         string jobName, domain;

         try
         {
            jobName = pathAndQuery.Field(1, '/');

            // Either without referrer using an absolute URL or from another host using an absolute URL.
            // Examples referrer -> requested URL -> translated address:
            // none -> http://mimeo/job1/site1/file1.html -> site1/file1.html in job1
            // http://not-mimeo/ -> http://mimeo/job1/site1/file1.html -> site1/file1.html in job1
            if (referrer == null || referrer.Authority != url.Authority) // Globals.HostName)
            {
               var canonizedUrl = new AtraxUrl(pathAndQuery.Skip(jobName.Length + 2)).Canonize();
               return Tuple.Create(jobName, canonizedUrl, false);
            }

            // Coming from Mimeo using an absolute URL
            // Examples:
            // http://mimeo/job1/site1/ -> http://mimeo/job1/site1/file1.html -> site1/file1.html in job1
            // http://mimeo/job1/site1/ -> http://mimeo/job2/site1/file1.html -> site1/file1.html in job2
            domain = pathAndQuery.Field(2, '/');
            if (_bucketUris.ContainsKey(jobName) && _domains[jobName].Contains(domain))
            {
               var canonizedUrl = new AtraxUrl(pathAndQuery.Skip(jobName.Length + 2)).Canonize();
               return Tuple.Create(jobName, canonizedUrl, false);
            }
         }
         catch (IndexOutOfRangeException)
         {
            // URL does not contain enough fields to extract jobName and domain. Try using the referrer URL.
         }
         catch (ArgumentOutOfRangeException)
         {
            // URL does not contain enough fields to extract jobName and domain. Try using the referrer URL.
         }

         if (referrer == null)
         {
            return null;
         }

         // Coming from Mimeo using a relative URL. Use referrer and rawUrl to determine the true address.
         // All relative URLs refer to the same job as the referrer so use the referrer to determine jobName.
         jobName = referrer.AbsolutePath.Field(1, '/');
         domain = referrer.AbsolutePath.Field(2, '/');

         if (_bucketUris.ContainsKey(jobName) && _domains[jobName].Contains(domain))
         {
            if (pathAndQuery.StartsWith("/" + jobName))
            {
               pathAndQuery = pathAndQuery.Substring(("/" + jobName).Length);
            }

            if (pathAndQuery.StartsWith("/" + domain))
            {
               pathAndQuery = pathAndQuery.Substring(("/" + domain).Length);
            }

            var canonizedUrl = new AtraxUrl(domain + pathAndQuery);
            return Tuple.Create(jobName, canonizedUrl, true);
         }

         return null;
      }

      /// <summary>
      /// Given the input figure out what the S3 address is for the requested resource.
      /// </summary>
      /// <param name="referrer"></param>
      /// <param name="url"></param>
      /// <returns>A Tuple (job name, URL of the resource in S3)</returns>
      public static Tuple<string, Uri, string> ResolveRepoAddress(Uri referrer, Uri url)
      {
         var destination = TranslateAddress(referrer, url);
         if (destination == null)
         {
            throw new MimeoNotFound();            
         }

         var jobName = destination.Item1;
         var canonizedUrl = destination.Item2;
         var redirectRequired = destination.Item3;

         var redirectTo = SiteCrawlerUtils.GetRedirectEndpoint(_sdb, new CrawlJob(jobName), canonizedUrl);
         if (redirectTo != canonizedUrl)
         {
            throw new MimeoRedirect("/" + jobName + "/" + redirectTo);
         }

         Uri s3WebsiteBaseUrl;
         if (!_bucketUris.TryGetValue(jobName, out s3WebsiteBaseUrl))
         {
            throw new MimeoNotFound();
         }

         if (redirectRequired)
         {
            throw new MimeoRedirect("/" + jobName + "/" + canonizedUrl);
         }

         return Tuple.Create(jobName, s3WebsiteBaseUrl, canonizedUrl.S3Key);
      }

      /// <summary>
      /// Get the resource from S3
      /// </summary>
      /// <param name="repoAddress"></param>
      /// <param name="key"></param>
      /// <returns>A Tuple (job name, web response object)</returns>
      public static HttpWebResponse FetchFromRepo(string repoAddress, string key)
      {
         var request = WebRequest.Create(repoAddress + SiteCrawlerUtils.EncodeS3Key(key));
         HttpWebResponse response;

         try
         {
            response = request.GetResponse() as HttpWebResponse;

            var original = response.GetResponseHeader("x-amz-meta-original");
            if (original != "self")
            {
               request = WebRequest.Create(repoAddress + SiteCrawlerUtils.EncodeS3Key(original));
               response = request.GetResponse() as HttpWebResponse;
            }
         }
         catch (WebException ex)
         {
            if (ex.Status == WebExceptionStatus.ProtocolError)
            {
               if ((ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound)
               {
                  throw new MimeoNotFound(ex.Message);
               }
               throw;
            }

            throw;
         }

         return response;
      }
   }
}
