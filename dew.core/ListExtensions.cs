using System;
using System.Collections.Generic;

public static class ListExtensions
{
	public static List<T> FilterInPlace<T>(this List<T> list, Func<T, bool> predicate)
	{
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (!predicate(list[num]))
			{
				list.RemoveAt(num);
			}
		}
		return list;
	}
}
