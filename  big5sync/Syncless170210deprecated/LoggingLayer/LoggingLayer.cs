using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Logging
{
    public class LoggingLayer
    {
        private static LoggingLayer _instance;
        public LoggingLayer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LoggingLayer();
                }
                return _instance;
            }
        }
        private LoggingLayer()
        {

        }
    }
}
