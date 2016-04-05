using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace ProphetsWay.Utilities
{
	[Flags]
	public enum LogLevels
	{
		NoLogging = 0,
		Error = 1,
		Warning = 3,
		Information = 7,
		Debug = 15
	}

	public static class Logger
	{
		private static IList<LoggingDestination> _destinations;

		private static IList<LoggingDestination> LoggingDestinations
		{
			get { return _destinations ?? (_destinations = new List<LoggingDestination>()); }
		}

		public static void AddDestination(LoggingDestination newDest)
		{
			LoggingDestinations.Add(newDest);
		}

		public static void ClearDestinations()
		{
			LoggingDestinations.Clear();
		}

		public static void Info(string message)
		{
			//Log(string.Format("{0}{1}", "INFORMATION:  ".PadLeft(15), message), LogLevels.Information);
			Log(message, LogLevels.Information);
		}

		public static void Debug(string message)
		{
			//Log(string.Format("{0}{1}", "DEBUG:  ".PadLeft(15), message), LogLevels.Debug);
			Log(message, LogLevels.Debug);
		}

		public static void Error(Exception ex, string generalMessage = "")
		{
			//Log(string.Format("{0}{1}\r\n{2}\r\n{3}", "ERROR:  ".PadLeft(15), generalMessage, ex.Message, ex.StackTrace), LogLevels.Error);
			string exMessage, exStackTrace;
			ExceptionLogExtractor(ex, out exMessage, out exStackTrace);

			Log(string.Format("{0}\r\n{1}\r\n{2}", generalMessage, exMessage, exStackTrace), LogLevels.Error);
		}

		public static void Warn(string message, Exception ex = null)
		{
			if (ex == null)
				//Log(string.Format("{0}{1}", "WARNING:  ".PadLeft(15), message), LogLevels.Warning);
				Log(message, LogLevels.Warning);
			else
			{
				string exMessage, exStackTrace;
				ExceptionLogExtractor(ex, out exMessage, out exStackTrace);

				//Log(string.Format("{0}{1}\r\n{2}\r\n{3}", "WARNING:  ".PadLeft(15), message, ex.Message, ex.StackTrace), LogLevels.Warning);
				Log(string.Format("{0}\r\n{1}\r\n{2}", message, exMessage, exStackTrace), LogLevels.Warning);
			}
		}

		private static void Log(string message, LogLevels level)
		{
			//var formattedMessage = string.Format("{0} :: {1}", DateTime.Now, message);

			if (LoggingDestinations.Count == 0)
				AddDestination(new EventLogDestination());

			foreach (var dest in LoggingDestinations)
				//dest.Log(formattedMessage, level);
				dest.Log(message, level);
		}

		private static void ExceptionLogExtractor(Exception ex, out string message, out string stack)
		{
			var imessage = string.Empty;
			var istack = string.Empty;
			if (ex.InnerException != null)
				ExceptionLogExtractor(ex.InnerException, out imessage, out istack);


			message = string.IsNullOrEmpty(imessage)
				? ex.Message
				: string.Format("{0}{1}{1}Inner Exception Message:{1}{2}", ex.Message, Environment.NewLine, imessage);

			stack = string.IsNullOrEmpty(istack)
				? ex.StackTrace
				: string.Format("{0}{1}{1}Inner Exception Stack Trace:{1}{2}", ex.StackTrace, Environment.NewLine, istack);
		}
	}

	public abstract class LoggingDestination
	{
		protected object LoggerLock = new object();

		private readonly LogLevels _reportingLevel;

		protected LoggingDestination(LogLevels reportingLevel)
		{
			_reportingLevel = reportingLevel;
		}

		public void Log(string message, LogLevels level)
		{
			if ((level & _reportingLevel) < level)
				return;

			LogStatement(message, level);
		}

		protected abstract void LogStatement(string message, LogLevels level);
	}

	public abstract class TextLogger : LoggingDestination
	{
		protected TextLogger(LogLevels reportingLevel) : base(reportingLevel)
		{
		}

		protected override void LogStatement(string message, LogLevels level)
		{
			//var formattedMessage = string.Format("{0} :: {1}", DateTime.Now, message);
			//Log(string.Format("{0}{1}", "WARNING:  ".PadLeft(15), message), LogLevels.Warning);

			var msg = string.Format("{0} :: {1}:  {2}", DateTime.Now, level.ToString().PadLeft(12), message);
			LogStatement(msg);
		}

		protected abstract void LogStatement(string message);
	}

	public class ConsoleDestination : TextLogger
	{
		public ConsoleDestination(LogLevels level = LogLevels.Debug) : base(level)
		{
		}

		protected override void LogStatement(string message)
		{
			lock (LoggerLock)
				Console.WriteLine(message);
		}
	}

	public class UserInterfaceDestination : LoggingDestination
	{
		public UserInterfaceDestination(LogLevels reportingLevel) : base(reportingLevel)
		{

		}

		protected override void LogStatement(string message, LogLevels level)
		{
			LoggingEvent.BeginInvoke(this, new LoggerArgs { LogLevel = level, Message = message }, EndAsyncLogEvent, null);
		}

		private static void EndAsyncLogEvent(IAsyncResult iar)
		{
			var ar = iar as AsyncResult;
			if (ar == null)
				return;

			var invokedMethod = ar.AsyncDelegate as EventHandler<LoggerArgs>;

			try
			{
				invokedMethod?.EndInvoke(iar);
			}
			catch
			{
				// ignored
			}
		}

		public EventHandler<LoggerArgs> LoggingEvent;

		public class LoggerArgs : EventArgs
		{
			public string Message { get; set; }

			public LogLevels LogLevel { get; set; }
		}

	}

	public class FileDestination : TextLogger
	{
		public FileDestination(string fileName, LogLevels level = LogLevels.Debug, bool clearFile = true)
			: base(level)
		{
			var file = new FileInfo(fileName);

			try
			{
				if (file.Exists && !clearFile)
				{
					_writer = file.AppendText();
				}
				else
				{
					if (file.Exists)
						file.Delete();

					if (file.Directory != null && !file.Directory.Exists)
						file.Directory.Create();

					_writer = file.CreateText();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;
			}
		}

		private readonly StreamWriter _writer;

		protected override void LogStatement(string message)
		{
			lock (LoggerLock)
			{
				_writer.WriteLine(message);
				_writer.Flush();
			}
		}

	}

	public class EventLogDestination : LoggingDestination
	{
		private readonly string _source;
		private const string WINDOWS_EVENT_VIEW_LOG_NAME = "Application";
		private readonly bool _valid = false;

		public EventLogDestination() : this(null, LogLevels.Debug)
		{
		}

		public EventLogDestination(LogLevels logLevel) : this(null, logLevel)
		{
		}

		public EventLogDestination(string applicationName) : this(applicationName, LogLevels.Debug)
		{
		}

		public EventLogDestination(string applicationName, LogLevels logLevel)
			: base(logLevel)
		{
			try
			{
				if (string.IsNullOrEmpty(applicationName))
				{
					var a = Assembly.GetEntryAssembly();
					applicationName = a != null
						? a.GetName().Name
						: "Anonomous Application";
				}

				_source = applicationName.Replace(" ", "");

				if (!EventLog.SourceExists(_source))
					EventLog.CreateEventSource(_source, WINDOWS_EVENT_VIEW_LOG_NAME);

				_valid = true;
			}
			catch (Exception)
			{
				//it's sad, but if we can't actually log to the event log, don't let it crash the whole app
			}
		}

		protected override void LogStatement(string message, LogLevels level)
		{
			if (!_valid)
				return;

			EventLogEntryType type;

			switch (level)
			{
				case LogLevels.Debug:
				case LogLevels.Information:
					type = EventLogEntryType.Information;
					break;

				case LogLevels.Warning:
					type = EventLogEntryType.Warning;
					break;

				case LogLevels.Error:
					type = EventLogEntryType.Error;
					break;

				default:
					return;
			}

			EventLog.WriteEntry(_source, message, type);
		}
	}
}