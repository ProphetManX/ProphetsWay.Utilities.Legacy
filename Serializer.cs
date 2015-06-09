using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;

namespace ProphetsWay.Utilities
{
	public static class Serializer
	{
		public static void SerializeAsToFile(this object objectToSerialize, string targetFileName)
		{
			var s = File.Open(targetFileName, FileMode.Create);

			objectToSerialize.SerializeAsByteArr().WriteTo(s);
			s.Flush();
			s.Close();
		}

		public static string SerializeAsXml(this object objectToSerialize)
		{
			var formatter = new XmlSerializer(objectToSerialize.GetType());
			var s = new StringWriter();
			var x = new XmlTextWriter(s);

			formatter.Serialize(x, objectToSerialize);

			return s.ToString();
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

		/*
		public static T DeserializeFromXml<T>(this XmlDocument xmlDocument)
		{
			var formatter = new XmlSerializer(typeof(T));
			XmlParserContext context = new XmlParserContext(xmlDocument.NameTable,);
			var r = new XmlTextReader(xmlDocument.InnerXml, XmlNodeType.Document, )
			var 

			var obj = (T)formatter.Deserialize(r);

			return obj;
		}
		//*/

		public static T DeserializeFromFile<T>(string targetFileName)
		{
			var s = File.Open(targetFileName, FileMode.Open);
			var obj = s.DeserializeFromByteArr<T>();

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

		/*
		public void SerializeObject(string filename, ObjectToSerialize objectToSerialize)
		{
			Stream stream = File.Open(filename, FileMode.Create);
			BinaryFormatter bFormatter = new BinaryFormatter();
			bFormatter.Serialize(stream, objectToSerialize);
			stream.Close();
		}

		public ObjectToSerialize DeSerializeObject(string filename)
		{
			ObjectToSerialize objectToSerialize;
			Stream stream = File.Open(filename, FileMode.Open);
			BinaryFormatter bFormatter = new BinaryFormatter();
			objectToSerialize = (ObjectToSerialize)bFormatter.Deserialize(stream);
			stream.Close();
			return objectToSerialize;
		}
		//*/




	}
}
