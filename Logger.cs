using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

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
			Log(string.Format("{0}{1}", "INFORMATION:  ".PadLeft(15), message), LogLevels.Information);
		}

		public static void Debug(string message)
		{
			Log(string.Format("{0}{1}", "DEBUG:  ".PadLeft(15), message), LogLevels.Debug);
		}

		public static void Error(Exception ex, string generalMessage = "")
		{
			Log(string.Format("{0}{1}\r\n{2}\r\n{3}", "ERROR:  ".PadLeft(15), generalMessage, ex.Message, ex.StackTrace),
				LogLevels.Error);
		}

		public static void Warn(string message, Exception ex = null)
		{
			if (ex == null)
				Log(string.Format("{0}{1}", "WARNING:  ".PadLeft(15), message), LogLevels.Warning);
			else
				Log(string.Format("{0}{1}\r\n{2}\r\n{3}", "WARNING:  ".PadLeft(15), message, ex.Message, ex.StackTrace),
					LogLevels.Error);
		}

		private static void Log(string message, LogLevels level)
		{
			var formattedMessage = string.Format("{0} :: {1}", DateTime.Now, message);

			if (LoggingDestinations.Count == 0)
				AddDestination(new EventLogDestination());

			foreach (var dest in LoggingDestinations)
				dest.Log(formattedMessage, level);
		}
	}

	public abstract class LoggingDestination
	{
		protected object LoggerLock = new object();

		private readonly LogLevels _reportingLevel;

		public LoggingDestination(LogLevels reportingLevel)
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

	public class ConsoleDestination : LoggingDestination
	{
		public ConsoleDestination(LogLevels level = LogLevels.Debug) : base(level)
		{
		}

		protected override void LogStatement(string message, LogLevels level)
		{
			lock (LoggerLock)
				Console.WriteLine(message);
		}
	}

	public class FileDestination : LoggingDestination
	{
		public FileDestination(string fileName, LogLevels level = LogLevels.Debug, bool clearFile = true)
			: base(level)
		{
			_file = new FileInfo(fileName);

			try
			{
				if (_file.Exists && !clearFile)
				{
					_writer = _file.AppendText();
				}
				else
				{
					if (_file.Exists)
						_file.Delete();

					if (!_file.Directory.Exists)
						_file.Directory.Create();

					_writer = _file.CreateText();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;
			}
		}

		private readonly FileInfo _file;
		private readonly StreamWriter _writer;

		protected override void LogStatement(string message, LogLevels level)
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
		private const string _log = "Application";

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
			if (string.IsNullOrEmpty(applicationName))
			{
				var a = Assembly.GetEntryAssembly();
				if (a != null)
					applicationName = a.GetName().Name;
				else
					applicationName = "Anonomous Application";

			}

			_source = applicationName.Replace(" ", "");

			if (!EventLog.SourceExists(_source))
				EventLog.CreateEventSource(_source, _log);
		}

		protected override void LogStatement(string message, LogLevels level)
		{
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