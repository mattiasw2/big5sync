﻿using System;
using log4net;
using Syncless.Core;

namespace Syncless.Logging
{
    public abstract class Logger
    {
        protected ILog log;

        public Logger(string name)
        {
            log = LogManager.GetLogger(name);
        }
        
        public abstract void Write(object message);
    }
}