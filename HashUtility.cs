using System;
using Microsoft.VisualBasic.Devices;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;

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
		private const string INVALID_HASH_TYPE = @"Improper value of HashType was used.";

		public static HashCollection GenerateHashes(this Stream stream)
		{
			var buffer = new byte[32 * 1024 * 1024];

			var md5Worker = new HashWorker(HashTypes.MD5);
			var sha1Worker = new HashWorker(HashTypes.SHA1);
			var sha256Worker = new HashWorker(HashTypes.SHA256);
			var sha512Worker = new HashWorker(HashTypes.SHA512);


			//event here to setup progressbar length

			var max = (int)(stream.Length / buffer.Length);

			var x = new ComputerInfo();
			var y = x.TotalPhysicalMemory;
			int bufferLength;

			bufferLength = stream.Read(buffer, 0, buffer.Length);
			max--;

			while (bufferLength > 0)
			{
				//process the buffer here...
				md5Worker.GenerateIncrementalHash(buffer, bufferLength);
				sha1Worker.GenerateIncrementalHash(buffer, bufferLength);
				sha256Worker.GenerateIncrementalHash(buffer, bufferLength);
				sha512Worker.GenerateIncrementalHash(buffer, bufferLength);

				bufferLength = stream.Read(buffer, 0, buffer.Length);
				max--;
			}

			md5Worker.GenerateIncrementalHash(buffer, bufferLength, true);
			sha1Worker.GenerateIncrementalHash(buffer, bufferLength, true);
			sha256Worker.GenerateIncrementalHash(buffer, bufferLength, true);
			sha512Worker.GenerateIncrementalHash(buffer, bufferLength, true);

			stream.Close();



			var ret = new HashCollection(md5Worker.Hash, sha1Worker.Hash, sha256Worker.Hash, sha512Worker.Hash);
			return ret;
		}

		public static string GenerateHash(this Stream stream, HashTypes hashType)
		{
			//HashAlgorithm crypto;
			//byte[] hash;

			//switch (hashType)
			//{
			//	case HashType.MD5:
			//		crypto = new MD5CryptoServiceProvider();
			//		break;

			//	case HashType.SHA1:
			//		crypto = new SHA1Managed();
			//		break;

			//	case HashType.SHA256:
			//		crypto = new SHA256Managed();
			//		break;

			//	case HashType.SHA512:
			//		crypto = new SHA512Managed();
			//		break;

			//	default:
			//		throw new InvalidEnumArgumentException(INVALID_HASH_TYPE);
			//}

			var worker = new HashWorker(hashType);
			worker.GenerateHash(stream);

			//try
			//{
			//	worker.Hash = worker.Hasher.ComputeHash(stream);
			//	//hash = crypto.ComputeHash(stream);
			//}
			//catch (Exception ex)
			//{
			//	Logger.Error(ex, string.Format("Error when trying to compute a [{0}] hash on a Stream.", hashType));
			//}
			//finally
			//{
			//	if (stream != null)
			//		stream.Close();
			//}

			return BitConverter.ToString(worker.Hash).Replace("-", "").ToLower();
		}
		
		public static bool VerifyHash(this Stream stream, string hash, HashTypes hashType)
		{
			return string.Equals(hash, GenerateHash(stream, hashType));
		}

		public static bool VerifyHash(this FileInfo fileInfo, string hash, HashTypes hashType)
		{
			return VerifyHash(fileInfo.OpenRead(), hash, hashType);
		}

		public static string GenerateHash(string fileName, HashTypes hashType)
		{
			return GenerateHash(new FileInfo(fileName).OpenRead(), hashType);
		}

		public static string GenerateHash(this FileInfo fileInfo, HashTypes hashType)
		{
			return GenerateHash(fileInfo.OpenRead(), hashType);
		}

		
	

		private class HashWorker
		{
			public int Offset { get; set; }
			public byte[] Hash
			{
				get
				{
					return Hasher.Hash;
				}
			}

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
					Logger.Error(ex, string.Format("Error when trying to compute a [{0}] hash on a Stream.", _hashType));
				}
				finally
				{
					if (stream != null)
						stream.Close();
				}
			}

			public int GenerateIncrementalHash(byte[] inputBuffer, int bufferLength, bool final = false)
			{
				if (!final)
					return Hasher.TransformBlock(inputBuffer, 0, bufferLength, inputBuffer, 0);
				else
					Hasher.TransformFinalBlock(inputBuffer, 0, bufferLength);
				
				return 0;
			}
		}
	}
}
