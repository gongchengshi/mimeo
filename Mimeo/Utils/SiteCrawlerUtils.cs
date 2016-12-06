using System.Collections.Generic;
using System.Linq;
using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;

namespace Mimeo.Utils
{
    public class CrawlJobGlossary
   {
      public readonly string CrawledUrls;
      public readonly string RedirectedUrls;

      public CrawlJobGlossary(string jobName)
      {
         CrawledUrls = jobName + ".crawled-urls";
         RedirectedUrls = jobName + ".redirected-urls";
      }
   };

   public class CrawlJob
   {
      public readonly CrawlJobGlossary Glossary;
      public readonly string Name;

      public CrawlJob(string jobName)
      {
         Name = jobName;
         Glossary = new CrawlJobGlossary(jobName);
      }
   }

   public static class SiteCrawlerUtils
   {
      private static readonly List<string> _redirectsToAttribute = new List<string> { "redirects_to" };

      public static AtraxUrl GetRedirectEndpoint(AmazonSimpleDBClient sdb, CrawlJob crawlJob, AtraxUrl referrer)
      {
         try
         {
            while (true)
            {
               var key = referrer.ToString();
               var response = sdb.GetAttributes(new GetAttributesRequest
               {
                  DomainName = crawlJob.Glossary.RedirectedUrls,
                  ItemName = key,
                  AttributeNames = _redirectsToAttribute
               });
               var attribute = response.Attributes.FirstOrDefault();

               if (attribute == null)
               {
                  // Try with or without the trailing slash
                  key = key.EndsWith("/") ? key.TrimEnd('/') : key + "/";

                  response = sdb.GetAttributes(new GetAttributesRequest
                  {
                     DomainName = crawlJob.Glossary.RedirectedUrls,
                     ItemName = key,
                     AttributeNames = _redirectsToAttribute
                  });
                  attribute = response.Attributes.FirstOrDefault();
               }

               if(attribute == null || attribute.Value == key)
               {
                  return referrer;
               }
               referrer = new AtraxUrl(attribute.Value);
            }
         }
         catch (AmazonSimpleDBException)
         {
            return referrer;
         }
      }

      public static string GetJobNameFromBucketName(string bucketName)
      {
         return bucketName.Substring(0, bucketName.IndexOf(".content"));
      }

      public static IEnumerable<S3Bucket> GetCrawledContentBuckets(AmazonS3Client s3)
      {
         var crawledContentBuckets = new List<S3Bucket>();
         foreach (var bucket in s3.ListBuckets().Buckets)
         {
            try
            {
               if (bucket.BucketName.EndsWith(".content"))
               {
                  crawledContentBuckets.Add(bucket);
               }
            }
            catch (AmazonS3Exception)
            {
               // The bucket does not reside in the same region as the S3 client.
            }
         }

         return crawledContentBuckets;
      }

//      public static S3Region GetBucketLocation(AmazonS3Client s3, string bucketName)
//      {
//         var getBucketLocationResponse = s3.GetBucketLocation(new GetBucketLocationRequest
//         {
//            BucketName = bucketName
//         });
//
//         return getBucketLocationResponse.Location;
//      }

//      public static AmazonS3Client GetS3Connection(AmazonS3Client s3, RegionEndpoint region)
//      {
//         return region == RegionEndpoint.USEast1 ? new AmazonS3Client(region) : s3;
//      }

      public static IEnumerable<string> GetDomains(AmazonS3Client s3, string bucketName)
      {
         var domains = new List<string>();
         var request = new ListObjectsRequest {BucketName = bucketName, Delimiter = @"/"};
         ListObjectsResponse response;
         do
         {
            response = s3.ListObjects(request);
            foreach (var directory in response.CommonPrefixes)
            {
//               yield return directory.TrimEnd('/');
               domains.Add(directory.TrimEnd('/'));
            }
            request.Marker = response.NextMarker;
         } while (response.IsTruncated);

         return domains;
      }

      /// <summary>
      /// .NET doesn't have an equivalent for Python's urllib.quote() which ignores '/'. 
      /// This provides the equivalent function. urllib.quote() is actually weird in the way it does this
      /// so this is really only applicable to this usage with AWS S3.
      /// 
      /// This is used when constructing a URL that points directly to an S3 file.
      /// </summary>
      /// <param name="key">Unencoded S3 Key</param>
      /// <returns>URL encoded S3 Key</returns>
      public static string EncodeS3Key(string key)
      {
         // Don't use HttpUtility.UrlEncode()! S3 is expecting hex encodings in upper case. HttpUtility uses lower case.
         return string.Join("/", key.Split('/').Select(WebUtility.UrlEncode));
      }
   }
}
