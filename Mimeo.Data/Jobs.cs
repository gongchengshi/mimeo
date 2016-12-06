using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace Mimeo.Data
{
   public class Job
   {
      public int Id { get; set; }
      [Required]
      public string Name { get; set; }
      [Required]
      public List<Domain> Domains { get; set; }
   }

   public class Domain
   {
      public int Id { get; set; }
      [Required]
      public string Name { get; set; }
      [Required]
      public string MimeoUrl { get; set; }
      public bool Redirects { get; set; }
   }
}
