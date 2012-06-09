using System;
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
			Log(string.Format("{0}{1}","DEBUG:  ".PadLeft(15), message), LogLevels.Debug);
		}

		public static void Error(Exception ex, string generalMessage = "")
		{
			Log(string.Format("{0}{1}\r\n{2}\r\n{3}", "ERROR:  ".PadLeft(15), generalMessage, ex.Message, ex.StackTrace), LogLevels.Error);
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
			var formattedMessage = string.Format("{0} :: {1}",DateTime.Now, message);

			foreach (var dest in LoggingDestinations)
			{
				dest.Log(formattedMessage, level);
			}
		}
	}

	public abstract class LoggingDestination
	{
		public abstract void Log(string message, LogLevels level);
	}

	public class ConsoleDestination : LoggingDestination
	{
		public ConsoleDestination(LogLevels level = LogLevels.Debug)
		{
			_logLevel = level;
		}

		private readonly LogLevels _logLevel;

		public override void Log(string message, LogLevels level)
		{
			if ((level & _logLevel) < level)
				return;

			Console.WriteLine(message);
		}
	}

	public class FileDestination : LoggingDestination
	{
		public FileDestination(string fileName, LogLevels level = LogLevels.Debug, bool clearFile = true)
		{
			_file = new FileInfo(fileName);
			_logLevel = level;

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

		private readonly LogLevels _logLevel;
		private readonly FileInfo _file;
		private readonly StreamWriter _writer;

		public override void Log(string message, LogLevels level)
		{
			if ((level & _logLevel) < level)
				return;

			_writer.WriteLine(message);
			_writer.Flush();
		}

	}
}