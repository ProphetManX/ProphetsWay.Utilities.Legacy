using System.IO;

namespace ProphetsWay.Utilities
{
	public static class Streamer
	{
		private const int BufferSize = 1024*1024; //a megabyte buffer

		public static byte[] ReadToEnd(this Stream source)
		{
			var ms = new MemoryStream();
			var bRead = 0;
			var buffer = new byte[BufferSize];

			do
			{
				bRead = source.Read(buffer, 0, buffer.Length);
				ms.Write(buffer, 0, bRead);
			} while (bRead > 0);

			return ms.ToArray();
		}

		public static Stream Streamify(this byte[] byteArr)
		{
			var ms = new MemoryStream(byteArr);
			return ms;
		}
	}
}