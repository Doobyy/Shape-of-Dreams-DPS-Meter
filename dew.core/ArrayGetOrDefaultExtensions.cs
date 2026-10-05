using System.Collections.Generic;

public static class ArrayGetOrDefaultExtensions
{
	public static T GetOrDefault<T>(this IList<T> array, int index, T defaultValue = default(T))
	{
		if (array == null || index < 0 || index >= array.Count)
		{
			return defaultValue;
		}
		return array[index];
	}

	public static bool TryGetValue<T>(this T[] array, int index, out T value)
	{
		if (array != null && index >= 0 && index < array.Length)
		{
			value = array[index];
			return true;
		}
		value = default;
		return false;
	}
}
