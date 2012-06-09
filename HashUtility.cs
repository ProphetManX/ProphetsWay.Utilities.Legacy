using System;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;

namespace ProphetsWay.Utilities
{
	public enum HashType
	{
		MD5,
		SHA1,
		SHA256,
		SHA512,
	}

	public class HashCollection
	{
		public HashCollection(string md5, string sha1, string sha256, string sha512)
		{
			MD5 = md5;
			SHA1 = sha1;
			SHA256 = sha256;
			SHA512 = sha512;
		}

		public string SHA1 { get; private set; }
		public string SHA256 { get; private set; }
		public string SHA512 { get; private set; }
		public string MD5 { get; private set; }
	}

	public static class HashUtility
	{
		private const string INVALID_HASH_TYPE = @"Improper value of HashType was used.";

		public static string GenerateHash(this Stream stream, HashType hashType)
		{
			HashAlgorithm crypto;
			byte[] hash;

			switch (hashType)
			{
				case HashType.MD5:
					crypto = new MD5CryptoServiceProvider();
					break;

				case HashType.SHA1:
					crypto = new SHA1Managed();
					break;

				case HashType.SHA256:
					crypto = new SHA256Managed();
					break;

				case HashType.SHA512:
					crypto = new SHA512Managed();
					break;

				default:
					throw new InvalidEnumArgumentException(INVALID_HASH_TYPE);
			}

			try
			{
				hash = crypto.ComputeHash(stream);
			}
			finally
			{
				if (stream != null)
					stream.Close();
			}

			return BitConverter.ToString(hash).Replace("-", "").ToLower();
		}
		
		public static bool VerifyHash(this Stream stream, string hash, HashType hashType)
		{
			return string.Equals(hash, GenerateHash(stream, hashType));
		}

		public static bool VerifyHash(this FileInfo fileInfo, string hash, HashType hashType)
		{
			return VerifyHash(fileInfo.OpenRead(), hash, hashType);
		}

		public static string GenerateHash(string fileName, HashType hashType)
		{
			return GenerateHash(new FileInfo(fileName).OpenRead(), hashType);
		}

		public static string GenerateHash(this FileInfo fileInfo, HashType hashType)
		{
			return GenerateHash(fileInfo.OpenRead(), hashType);
		}
	}
}
