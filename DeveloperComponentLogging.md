

# Overview #

The **Logging** component is in charge of handling all user, debug and developer logging actions. It is implemented using [log4net](http://logging.apache.org/log4net/).

# Classes #

The Logging component makes use of a third-party Dynamic Link Library (DLL), _log4net.dll_, to perform logging.

## LoggingLayer ##

This is the main logic layer which is called by `SystemLogicLayer` to perform the various logging operation. A `Logger` object is instantiated and returned to `SystemLogicLayer` when `SystemLogicLayer` call `GetLogger` method of `LoggingLayer`.

## Logger ##

There are three types of logger inheriting the `Logger` class. They are `UserLogger`, `DebugLogger` and `DeveloperLogger`. When retrieving a logger, a name is passed to the `LoggerFactory` to indicate the specific log you want to retrieve.

# Extending Logging #
Logging uses factory and decorator design patterns. This allows the developers to easily add new types of loggers when necessary and extend the method to do pre-processing and post-processing tasks before writing the log. Polymorphism is used so user can just use the Write method from 'Logger' to write logs.