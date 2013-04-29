using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using SemanticLog4Net.Formatters;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
	class Program
	{
		static void Main(string[] args)
		{
			var formatter = new Log4NetEventTextFormatter();
			var listener = new ObservableEventListener();
			listener.EnableEvents(SampleEventSource.Log, EventLevel.LogAlways, Keywords.All);
			listener.LogToFlatFile("test.xml", formatter, true);

			SampleEventSource.Log.ApplicationStarting();

			for (int i = 0; i < 20; i++)
				SampleEventSource.Log.SomeOtherEvent(i);

			try
			{
				throw new Exception("Uh oh, watch out");
			}
			catch (Exception e)
			{
				SampleEventSource.Log.Error(e);
			}

			SampleEventSource.Log.ApplicationStopping();
		}
	}

	class SampleEventSource : EventSource
	{
		public static SampleEventSource Log = new SampleEventSource();

		[Event(1, Message = "Starting up.")]
		public void ApplicationStarting()
		{
			this.WriteEvent(1);
		}

		[Event(2, Message = "Shutting down.")]
		public void ApplicationStopping()
		{
			this.WriteEvent(2);
		}

		[Event(3, Message = "Here's something amazing.")]
		public void SomeOtherEvent(int param)
		{
			this.WriteEvent(3, param);
		}

		[Event(4, Message = "Oops a problem!")]
		public void Error(string exception)
		{
			this.WriteEvent(4, exception);
		}

		[NonEvent]
		public void Error(Exception exception)
		{
			this.Error(exception.ToString());
		}
	}
}
