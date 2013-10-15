using System;

namespace ProphetsWay.Utilities
{
	public static class ObjectDuplicator
	{
	    /// <summary>
	    /// Will duplicate any object, and any properties that are reference types will also be duplicated.  Will not copy Lists nicely?
	    /// </summary>
	    /// <returns>Returns a copy of the input type.</returns>
	    public static T DuplicateObject<T>(this T original)
	        where T : new()
	    {
	        var type = typeof (T);
	        var copy = (T) Activator.CreateInstance(type, new object[] {});

	        var properties = type.GetProperties();

	        foreach (var property in properties)
	        {
	            if (!property.CanWrite)
	                continue;

	            var o = property.PropertyType.IsByRef
	                        ? property.GetValue(original, null).DuplicateObject()
	                        : property.GetValue(original, null);

	            if (o != null)
	                property.SetValue(copy, o, null);

	        }

	        return copy;
	    }

        public static T DuplicateObjectSerial<T>(this T original)
        {
            var copy = original.SerializeAsByteArr().DeserializeFromByteArr<T>();

            return copy;
        }

    }
}
