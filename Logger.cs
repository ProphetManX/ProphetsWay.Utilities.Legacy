﻿using System;
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
		private static readonly IList<LoggingDestination> _destinations = new List<LoggingDestination>();

		public static void AddDestination(LoggingDestination newDest)
		{
			_destinations.Add(newDest);
		}

		public static void ClearDestinations()
		{
			_destinations.Clear();
		}

		public static void Info(string message)
		{
			Log(message, LogLevels.Information);
		}

		public static void Debug(string message)
		{
			Log(message, LogLevels.Debug);
		}

		public static void Error(Exception ex, string generalMessage = "")
		{
			string exMessage, exStackTrace;
			ExceptionLogExtractor(ex, out exMessage, out exStackTrace);

			Log($"{generalMessage}\r\n{exMessage}\r\n{exStackTrace}", LogLevels.Error);
		}

		public static void Warn(string message, Exception ex = null)
		{
			if (ex == null)
				Log(message, LogLevels.Warning);
			else
			{
				string exMessage, exStackTrace;
				ExceptionLogExtractor(ex, out exMessage, out exStackTrace);

				Log($"{message}\r\n{exMessage}\r\n{exStackTrace}", LogLevels.Warning);
			}
		}

		private static void Log(string message, LogLevels level)
		{
			if (_destinations.Count == 0)
				AddDestination(new EventLogDestination());

			foreach (var dest in _destinations)
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
		protected readonly object LoggerLock = new object();

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
			var msg = $"{DateTime.Now} :: {level.ToString().PadLeft(12)}:  {message}";
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
			LoggingEvent.Invoke(this, new LoggerArgs { LogLevel = level, Message = message });
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
			_fi = new FileInfo(fileName);

			try
			{
				if (_fi.Exists && clearFile)
					_fi.Delete();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;
			}
		}

		private FileInfo _fi;

		protected override void LogStatement(string message)
		{
			using (var sr = _fi.Open(FileMode.OpenOrCreate, FileAccess.Write))
			{
				var msgBytes = message.SerializeAsByteArr();
				lock (LoggerLock)
				{
					msgBytes.WriteTo(sr);
					sr.Flush();
				}
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