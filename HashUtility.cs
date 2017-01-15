using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ProphetsWay.Utilities
{
	public enum HashTypes
	{
		MD5,
		SHA1,
		SHA256,
		SHA512,
	}

	public class HashCollection
	{
		public HashCollection(byte[] md5, byte[] sha1, byte[] sha256, byte[] sha512)
		{
			MD5 = BitConverter.ToString(md5).Replace("-", "").ToLower();
			SHA1 = BitConverter.ToString(sha1).Replace("-", "").ToLower();
			SHA256 = BitConverter.ToString(sha256).Replace("-", "").ToLower();
			SHA512 = BitConverter.ToString(sha512).Replace("-", "").ToLower();
		}

		public string SHA1 { get; private set; }
		public string SHA256 { get; private set; }
		public string SHA512 { get; private set; }
		public string MD5 { get; private set; }
	}

	public static class HashUtility
	{
		private const int BUFFER_SIZE = 1024*1024*128;

		private const string INVALID_HASH_TYPE = @"Improper value of HashType was used.";

		//public static HashCollection GenerateHashes(this Stream stream)
		//{
		//	var buffer = new byte[BUFFER_SIZE];

		//	var md5Worker = new HashWorker(HashTypes.MD5);
		//	var sha1Worker = new HashWorker(HashTypes.SHA1);
		//	var sha256Worker = new HashWorker(HashTypes.SHA256);
		//	var sha512Worker = new HashWorker(HashTypes.SHA512);


		//	//event here to setup progressbar length

		//	//var max = (int)(stream.Length / buffer.Length);

		//	//var x = new ComputerInfo();
		//	//var y = x.TotalPhysicalMemory;
		//	int bufferLength;

		//	do
		//	{
		//		bufferLength = stream.Read(buffer, 0, buffer.Length);
		//		//max--;

		//		//process the buffer here...
		//		md5Worker.GenerateIncrementalHash(buffer, bufferLength);
		//		sha1Worker.GenerateIncrementalHash(buffer, bufferLength);
		//		sha256Worker.GenerateIncrementalHash(buffer, bufferLength);
		//		sha512Worker.GenerateIncrementalHash(buffer, bufferLength);

		//	} while (bufferLength > 0);

		//	var ret = new HashCollection(md5Worker.Hash, sha1Worker.Hash, sha256Worker.Hash, sha512Worker.Hash);
		//	return ret;
		//}

		public static string GenerateHash(this Stream stream, HashTypes hashType)
		{
			var worker = new HashWorker(hashType);
			worker.GenerateHash(stream);

			return BitConverter.ToString(worker.Hash).Replace("-", "").ToLower();
		}

		public static bool VerifyHash(this Stream stream, string hash, HashTypes hashType)
		{
			return string.Equals(hash, GenerateHash(stream, hashType), StringComparison.OrdinalIgnoreCase);
		}

		public static bool VerifyHash(this FileInfo fileInfo, string hash, HashTypes hashType)
		{
			return VerifyHash(fileInfo.OpenRead(), hash, hashType);
		}

		public static string GenerateHash(string fileName, HashTypes hashType)
		{
			return GenerateHash(new FileInfo(fileName), hashType);
		}

		public static string GenerateHash(this FileInfo fileInfo, HashTypes hashType)
		{
			return GenerateHash(fileInfo.OpenRead(), hashType);
		}

		public static HashCollection GenerateHashes(this Stream stream)
		{
			var buffer = new byte[BUFFER_SIZE];
			var md5Worker = new HashWorker(HashTypes.MD5);
			var sha1Worker = new HashWorker(HashTypes.SHA1);
			var sha256Worker = new HashWorker(HashTypes.SHA256);
			var sha512Worker = new HashWorker(HashTypes.SHA512);


			Task.Run(() =>
			{
				var bRead = 0;
				do
				{
					bRead = stream.Read(buffer, 0, buffer.Length);

					var tasks = new List<Task>
					{
						Task.Run(() => md5Worker.GenerateIncrementalHash(buffer, bRead)),
						Task.Run(() => sha1Worker.GenerateIncrementalHash(buffer, bRead)),
						Task.Run(() => sha256Worker.GenerateIncrementalHash(buffer, bRead)),
						Task.Run(() => sha512Worker.GenerateIncrementalHash(buffer, bRead))
					};

					Task.WaitAll(tasks.ToArray());
				} while (bRead > 0);
			}).Wait();


			var ret = new HashCollection(md5Worker.Hash, sha1Worker.Hash, sha256Worker.Hash, sha512Worker.Hash);
			return ret;
		}

		//public static HashCollection GenerateHashesThreaded(this Stream stream)
		//{
		//	//I want 5 threads.
		//	//1 thread to read the file in and put it into buffers
		//	//4 other threads, 1 for each hash type, worker threads

		//	//I need the worker threads to wait and read from the input buffer
		//	//once all threads have read the buffer, clear the buffer's data (can leave the buffer item record)

		//	const int bufferChunkSize = BUFFER_SIZE;
		//	const int totalWorkers = 4;

		//	var md5Worker = new HashWorker(HashTypes.MD5);
		//	var sha1Worker = new HashWorker(HashTypes.SHA1);
		//	var sha256Worker = new HashWorker(HashTypes.SHA256);
		//	var sha512Worker = new HashWorker(HashTypes.SHA512);

		//	var args = new ReadStreamIntoBufferArgs
		//	{
		//		DataStream = stream,
		//		BufferArraySize = bufferChunkSize,
		//		SignOffsRequiredToClearBuffer = totalWorkers
		//	};

		//	Buffer = new List<BufferedData>();

		//	var thBuf = new Thread(ReadStreamIntoBuffer);
		//	thBuf.Start(args);

		//	md5Worker.GenerateThreadedHash();
		//	sha1Worker.GenerateThreadedHash();
		//	sha256Worker.GenerateThreadedHash();
		//	sha512Worker.GenerateThreadedHash();


		//	while (Buffer.Count == 0 || Buffer[Buffer.Count - 1].FinishedCount != totalWorkers)
		//		Thread.Sleep(100);

		//	var ret = new HashCollection(md5Worker.Hash, sha1Worker.Hash, sha256Worker.Hash, sha512Worker.Hash);
		//	return ret;
		//}

		//private static List<BufferedData> Buffer { get; set; }

		//private class BufferedData
		//{
		//	//public BufferedData(byte[] bufferData, int bufferLength, int signOffsRequiredToDelete)
		//	//{
		//	//	_signOffReq = signOffsRequiredToDelete;
		//	//	_signOffCount = 0;
		//	//	FinishedCount = 0;

		//	//	Data = bufferData;
		//	//	Length = bufferLength;
		//	//}

		//	public byte[] Data { get; private set; }

		//	public int Length { get; private set; }

		//	private readonly int _signOffReq;
		//	private int _signOffCount;

		//	public void SignOff()
		//	{
		//		_signOffCount++;

		//		if (_signOffCount >= _signOffReq)
		//			Data = null;
		//	}

		//	public int FinishedCount { get; private set; }

		//	public void Finished()
		//	{
		//		FinishedCount++;
		//	}

		//}

		private class HashWorker
		{
			public byte[] Hash => Hasher.Hash;

			private HashAlgorithm Hasher { get; set; }

			private readonly HashTypes _hashType;

			public HashWorker(HashTypes hashType)
			{
				_hashType = hashType;

				switch (_hashType)
				{
					case HashTypes.MD5:
						Hasher = new MD5CryptoServiceProvider();
						break;

					case HashTypes.SHA1:
						Hasher = new SHA1Managed();
						break;

					case HashTypes.SHA256:
						Hasher = new SHA256Managed();
						break;

					case HashTypes.SHA512:
						Hasher = new SHA512Managed();
						break;

					default:
						throw new InvalidEnumArgumentException(INVALID_HASH_TYPE);
				}
			}

			public void GenerateHash(Stream stream)
			{
				try
				{
					Hasher.ComputeHash(stream);
				}
				catch (Exception ex)
				{
					Logger.Error(ex, $"Error when trying to compute a [{_hashType}] hash on a Stream.");
				}
			}

			public void GenerateIncrementalHash(byte[] inputBuffer, int bufferLength)
			{
				if (bufferLength > 0)
					Hasher.TransformBlock(inputBuffer, 0, bufferLength, inputBuffer, 0);
				else
					Hasher.TransformFinalBlock(inputBuffer, 0, bufferLength);
			}


			//public void GenerateThreadedHash()
			//{
			//	var th = new Thread(GenerateHashThreaded);
			//	th.Start();
			//}

			//private void GenerateHashThreaded()
			//{
			//	int length;
			//	var currChunk = 0;

			//	do
			//	{
			//		while (Buffer.Count <= currChunk)
			//		{
			//			Thread.Sleep(10);
			//		}

			//		var data = Buffer[currChunk].Data;
			//		length = Buffer[currChunk].Length;
			//		Buffer[currChunk].SignOff();

			//		GenerateIncrementalHash(data, length);

			//		Buffer[currChunk].Finished();

			//		currChunk++;
			//	} while (length > 0);
			//}
		}

		//private class ReadStreamIntoBufferArgs
		//{
		//	public int BufferArraySize { get; set; }

		//	public Stream DataStream { get; set; }

		//	public int SignOffsRequiredToClearBuffer { get; set; }
		//}

		//private static void ReadStreamIntoBuffer(object args)
		//{
		//	var tArgs = (ReadStreamIntoBufferArgs)args;
		//	int bufferLength;

		//	do
		//	{
		//		var buffer = new byte[tArgs.BufferArraySize];

		//		bufferLength = tArgs.DataStream.Read(buffer, 0, buffer.Length);
		//		Buffer.Add(new BufferedData(buffer, bufferLength, tArgs.SignOffsRequiredToClearBuffer));

		//	} while (bufferLength > 0);
		//}

	}
}