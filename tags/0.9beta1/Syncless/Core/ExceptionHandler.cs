using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Logging;
namespace Syncless.Core
{
    public class ExceptionHandler
    {
        public static void Handle(Exception e)
        {
            Logger logger = ServiceLocator.Getlogger(ServiceLocator.DEBUG_LOG);
            logger.WriteLine("Unexpected Exception Happened : ");
            logger.WriteLine(e.Message);
            logger.WriteLine("----------- Stack Trace -----------");
            logger.WriteLine(e.StackTrace);
            logger.WriteLine("-----------------------------------");

        }
    }
}
