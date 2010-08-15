using System;
using System.IO;
using System.Text;

namespace ProphetsWay.Utilities
{
	public static class FileTools
	{
		private const int READ_BUFFER_SIZE = 512 * 1024;
		private const int WRITE_BUFFER_SIZE = 512 * 1024;

		public static void CopyFast(this FileInfo fileInfo, string requestedFullName)
		{
			try
			{
				Logger.Debug(string.Format("COPYING file [{0}] to location [{1}].", fileInfo.FullName, requestedFullName));

				var buffer = new byte[READ_BUFFER_SIZE];
				using (var outStream = File.OpenWrite(requestedFullName))
				{
					using (var inStream = fileInfo.OpenRead())
					{
						int bytesRead;
						while ((bytesRead = inStream.Read(buffer, 0, READ_BUFFER_SIZE)) != 0)
						{
							var written = 0;
							while(written < bytesRead)
							{
								var toWrite = Math.Min(WRITE_BUFFER_SIZE, bytesRead - written);
								outStream.Write(buffer, written, toWrite);
								written += toWrite;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex, string.Format("Unable to move file [{0}] to location [{1}]", fileInfo.FullName, requestedFullName));
				throw;
			}
		}
		public static void MoveWithUniqueName(this FileInfo fileInfo, string requestedFullName)
		{
			var newName = CheckAndRenameFile(requestedFullName);

			try
			{
				Logger.Debug(string.Format("MOVING file [{0}] to location [{1}].", fileInfo.FullName, newName));
				fileInfo.MoveTo(newName);
			}
			catch (Exception ex)
			{
				Logger.Error(ex, string.Format("Unable to move file [{0}] to location [{1}]", fileInfo.FullName, newName));
				throw;
			}
		}

		public static void CopyWithUniqueName(this FileInfo fileInfo, string requestedFullName)
		{
			var newName = CheckAndRenameFile(requestedFullName);
			fileInfo.CopyFast(newName);
		}

		/// <summary>
		/// Takes a requested file name, checks to see if it already exists, and if it does, 
		/// appends " ~ " with a number behind until it finds a name that isn't taken.
		/// </summary>
		public static string CheckAndRenameFile(string requestedFileName)
		{
			Logger.Debug(string.Format("{0}{1}", "Requested File Name:  ".PadLeft(40), requestedFileName));

			var newFileName = requestedFileName;
			var file = new FileInfo(requestedFileName);

			if (!Directory.Exists(file.DirectoryName) && file.DirectoryName != null)
				Directory.CreateDirectory(file.DirectoryName);
		

			if (file.Exists)
			{
				Logger.Debug("Requested File already exists, attempting Rename...");
				var baseName = string.Format("{0}{1}", newFileName.Replace(file.Extension, ""), " ~ ");
				var baseExt = file.Extension;

				var i = 2;

				for (; ; i++)
				{
					file = new FileInfo(string.Format("{0}{1}{2}", baseName, IntToStringWithPlaceHolders(i, 2), baseExt));
					Logger.Debug(string.Format("{0}{1}", "Checking if Name Exists:  ".PadLeft(40), file.FullName));
					if (!file.Exists)
						break;
				}

				newFileName = file.FullName;
			}

			Logger.Debug(string.Format("{0}{1}", "Valid Name Found:  ".PadLeft(40), newFileName));
			return newFileName;
		}

		private static string IntToStringWithPlaceHolders(int inputValue, int numberOfPlaces)
		{
			var stringValue = inputValue.ToString();
			var missingPlaces = numberOfPlaces - stringValue.Length;

			if (missingPlaces > 0)
			{
				var placeHolders = new StringBuilder();
				for (var i = 0; i < missingPlaces; i++)
					placeHolders.Append("0");

				return placeHolders + stringValue;
			}

			return stringValue;
		}
	}
}
