using System;

namespace Mimeo.Utils
{
   public class MimeoException : Exception
   {
      public MimeoException()
      { }

      public MimeoException(string message)
         : base(message)
      { }
   }

   public class MimeoRedirect : MimeoException
   {
      public string Destination { get; private set; }

      public MimeoRedirect(string destination)
      {
         Destination = destination;
      }
   }

   public class MimeoNotFound : MimeoException
   {
      public MimeoNotFound()
      { }

      public MimeoNotFound(string message)
         : base(message)
      { }
   }

   public class MimeoExternalDestination : MimeoException
   {
      public string Destination { get; private set; }

      public MimeoExternalDestination(string destination)
      {
         Destination = destination;
      }
   }

   public class NavigateToMimeoRoot : MimeoException
   {}

   public class PrintMessageToClient : MimeoException
   {
      public PrintMessageToClient(string message) : base(message)
      {}
   }
}
