using System.Data.Entity;

namespace Mimeo.Data
{
   public class MimeoDb : DbContext
   {
      public MimeoDb() : base("Mimeo")
      {}

      public DbSet<Job> Jobs { get; set; }
      public DbSet<Domain> Domains { get; set; }
   }
}
