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
            ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
        }
    }
}
