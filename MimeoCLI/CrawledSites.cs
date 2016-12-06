using System.Collections.Generic;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleDB;
using Mimeo.Utils;

namespace Mimeo
{
    static public class CrawledSites
   {
      private static readonly AmazonS3Client _s3 = new AmazonS3Client(RegionEndpoint.USWest2);
      private static readonly AmazonSimpleDBClient _sdb = new AmazonSimpleDBClient(RegionEndpoint.USWest2);

      public static Dictionary<string, Dictionary<string, System.Tuple<string, bool>>> GetJobs(IList<string> jobNames)
      {
         var jobs = new Dictionary<string, Dictionary<string, System.Tuple<string, bool>>>();

         foreach (var bucket in SiteCrawlerUtils.GetCrawledContentBuckets(_s3))
         {
            var jobName = SiteCrawlerUtils.GetJobNameFromBucketName(bucket.BucketName);

            if (!jobNames.Contains(jobName))
            {
               continue;
            }

            if (!jobs.ContainsKey(jobName))
            {
               jobs[jobName] = new Dictionary<string, System.Tuple<string, bool>>();
            }

            var request = new ListObjectsRequest {BucketName = bucket.BucketName, Delimiter = @"/"};

            ListObjectsResponse response;
            do
            {
               response = _s3.ListObjects(request);
               foreach (var directory in response.CommonPrefixes)
               {
                  var domain = directory.TrimEnd('/');
                  var endpoint = SiteCrawlerUtils.GetRedirectEndpoint(_sdb, new CrawlJob(jobName), new AtraxUrl(directory));
                  var endpointStr = endpoint.ToString();
                  bool isRedirect = domain.TrimEnd('/') != endpointStr.TrimEnd('/');
                  jobs[jobName][domain] = System.Tuple.Create(endpointStr.TrimEnd('/'), isRedirect);
               }
               request.Marker = response.NextMarker;
            } while (response.IsTruncated);
         }

         return jobs;
      }
   }
}
