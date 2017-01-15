using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace ProphetsWay.Utilities
{
	public static class Serializer
	{
		public static void SerializeAsToFile(this object objectToSerialize, string targetFileName)
		{
			using (var ms = objectToSerialize.SerializeAsByteArr())
			using (var s = File.Open(targetFileName, FileMode.Create))
			{
				ms.WriteTo(s);
				s.Flush();
				s.Close();
			}
		}

		public static string SerializeAsXml(this object objectToSerialize)
		{
			var formatter = new XmlSerializer(objectToSerialize.GetType());
			var s = new StringWriter();
			var x = new XmlTextWriter(s);

			formatter.Serialize(x, objectToSerialize);

			return s.ToString();
		}

		public static MemoryStream SerializeAsByteArr(this string stringToSerialize)
		{
			var bytes = Encoding.UTF8.GetBytes(stringToSerialize);
			var ms = new MemoryStream(bytes);
			return ms;
		}

		public static MemoryStream SerializeAsByteArr(this object objectToSerialize)
		{
			var formatter = new BinaryFormatter();
			var s = new MemoryStream();

			formatter.Serialize(s, objectToSerialize);
			s.Flush();
			s.Position = 0;

			return s;
		}

		public static string SerializeAsJSON(this object objectToSerialize)
		{
			return JsonConvert.SerializeObject(objectToSerialize);
		}

		public static T DeserializeFromJSON<T>(this string jsonText)
		{
			return JsonConvert.DeserializeObject<T>(jsonText);
		}

		public static T DeserializeFromXml<T>(this XmlDocument xmlDocument)
		{
			var xmlBytes = xmlDocument.OuterXml.SerializeAsByteArr();
			var formatter = new XmlSerializer(typeof(T));
			var obj = (T)formatter.Deserialize(xmlBytes);

			return obj;
		}

		public static T DeserializeFromFile<T>(string targetFileName)
		{
			T obj;

			using (var s = File.Open(targetFileName, FileMode.Open))
				obj = s.DeserializeFromByteArr<T>();

			return obj;
		}

		public static T DeserializeFromByteArr<T>(this Stream binaryStream)
		{
			var formatter = new BinaryFormatter();
			var obj = (T)formatter.Deserialize(binaryStream);
			binaryStream.Close();

			return obj;
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
