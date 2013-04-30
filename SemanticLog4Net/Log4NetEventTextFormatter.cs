using log4net;
using log4net.Core;
using log4net.Layout;
using log4net.Repository;
using log4net.Util;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Formatters;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.Reflection;

namespace SemanticLog4Net.Formatters
{
	public class Log4NetEventTextFormatter : IEventTextFormatter
	{
		readonly XmlLayout xmlLayout;
		readonly ILoggerRepository repository;

		public Log4NetEventTextFormatter()
		{
			this.xmlLayout = new XmlLayout();
			this.repository = LogManager.GetRepository(Assembly.GetCallingAssembly());
		}

		public void WriteEvent(EventEntry eventEntry, System.IO.TextWriter writer)
		{
			var loggingEventData = new LoggingEventData
			{
				Level = this.GetLevel(eventEntry.Schema.Level),
				LoggerName = eventEntry.Schema.ProviderName + '.' + eventEntry.Schema.TaskName,
				Message = eventEntry.FormattedMessage,
				TimeStamp = eventEntry.Timestamp.DateTime,
			};

			if (eventEntry.Payload.Count > 0)
			{
				loggingEventData.Properties = this.GetPropertiesDictionary(eventEntry.Schema.Payload, eventEntry.Payload);
				loggingEventData.ExceptionString = this.GetExceptionString(eventEntry.Schema.Payload, eventEntry.Payload);
			}

			var loggingEvent = new LoggingEvent(null, this.repository, loggingEventData);
			this.xmlLayout.Format(writer, loggingEvent);
		}

		PropertiesDictionary GetPropertiesDictionary(string[] payloadNames, ReadOnlyCollection<object> payloadValues)
		{
			var propertiesDictionary = new log4net.Util.PropertiesDictionary();
			for (int i = 0; i < payloadNames.Length; i++)
				propertiesDictionary[payloadNames[i]] = payloadValues[i];
			return propertiesDictionary;
		}

		string GetExceptionString(string[] payloadNames, ReadOnlyCollection<object> payloadValues)
		{
			for (int i = 0; i < payloadNames.Length; i++)
				if (payloadNames[i] == "exception")
				{
					var exception = payloadValues[i] as string;
					if (exception != null)
						return exception;
				}
			return null;
		}

		Level GetLevel(EventLevel level)
		{
			switch (level)
			{
				case EventLevel.Critical:
					return Level.Critical;
				case EventLevel.Error:
					return Level.Error;
				case EventLevel.Informational:
					return Level.Info;
				case EventLevel.LogAlways:
					return Level.Off;
				case EventLevel.Verbose:
					return Level.Trace;
				case EventLevel.Warning:
					return Level.Warn;
				default:
					throw new InvalidOperationException("Unknown EventLevel.");
			}
		}
	}

}
