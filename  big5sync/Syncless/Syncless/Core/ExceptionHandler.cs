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
            Logger logger = ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG);
            logger.Write("Unexpected Exception Happened : ");
            logger.Write(e.Message);
            logger.Write("----------- Stack Trace -----------");
            logger.Write(e.StackTrace);
            logger.Write("-----------------------------------");

        }
    }
}
