using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
		}

	}
}