using System.IO;
using Newtonsoft.Json;

namespace ProphetsWay.Utilities
{
	/// <summary>
	/// Basically, comment out these classes unless we actually use the protocol buffers or newtonsoft JSON libraries
	/// </summary>
	public static class ExternalSerializers
	{
		 

		public static string SerializeAsJSON(this object objectToSerialize)
		{
			return JsonConvert.SerializeObject(objectToSerialize);
		}

		public static T DeserializeFromJSON<T>(this string jsonText)
		{
			return JsonConvert.DeserializeObject<T>(jsonText);
		}



		public static MemoryStream SerializeAsProtobuf(this object objectToSerialize)
		{
			var s = new MemoryStream();
			ProtoBuf.Serializer.Serialize(s, objectToSerialize);

			s.Flush();
			s.Position = 0;

			return s;
		}

		public static T DeserializeFromProtobuf<T>(this Stream protoStream)
		{
			var obj = ProtoBuf.Serializer.Deserialize<T>(protoStream);
			return obj;
		}




	}
}