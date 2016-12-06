using System.Collections.Generic;
using System.Data.Entity;
using Mimeo.Data;

namespace Mimeo
{
   class Program
   {
      static void Main(string[] args)
      {
         Database.SetInitializer(new CreateDatabaseIfNotExists<MimeoDb>());
         // Database.SetInitializer(new DropCreateDatabaseAlways<MimeoDb>());

         var db = new MimeoDb();

         var jobs = CrawledSites.GetJobs(new[] { "sel11122014", "redshed12052014" });

         foreach (var job in jobs)
         {
            var jobEntity = new Job
               {
                  Name = job.Key,
                  Domains = new List<Domain>(job.Value.Count)
               };
            foreach (var item in job.Value)
            {
               jobEntity.Domains.Add(new Domain
                  {
                     Name = item.Key,
                     MimeoUrl = item.Value.Item1,
                     Redirects = item.Value.Item2
                  });
            }
            db.Jobs.Add(jobEntity);
         }
         db.SaveChanges();
      }
   }
}
