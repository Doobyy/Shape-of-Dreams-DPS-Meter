using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

public static class LeveledPropertyTools
{
	public static void SetPropertyOrFieldValue(object target, string name, string value)
	{
		Type type = target.GetType();
		PropertyInfo property = type.GetProperty(name);
		FieldInfo field = type.GetField(name);
		if (property == null && field == null)
		{
			throw new Exception($"Field or property with name '{name}' not found on '{target}'");
		}
		if (property != null && field != null)
		{
			throw new Exception("How is this even possible lmao");
		}
		object value2 = TypeDescriptor.GetConverter((property != null) ? property.PropertyType : field.FieldType).ConvertFromString(value);
		if (property != null)
		{
			property.SetValue(target, value2);
		}
		else
		{
			field.SetValue(target, value2);
		}
	}

	public static float[] ParseFloatValues(string values)
	{
		string[] array = values.Split('/', StringSplitOptions.None);
		float[] array2 = new float[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = float.Parse(array[i], CultureInfo.InvariantCulture);
		}
		return array2;
	}

	public static int[] ParseIntValues(string values)
	{
		string[] array = values.Split('/', StringSplitOptions.None);
		int[] array2 = new int[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = int.Parse(array[i], CultureInfo.InvariantCulture);
		}
		return array2;
	}
}
