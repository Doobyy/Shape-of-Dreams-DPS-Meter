using System.Collections.Generic;

public static class ToListNonAllocExtensions
{
	public static List<T> ToListNonAlloc<T>(this IEnumerable<T> enumerable, out ListReturnHandle<T> handle)
	{
		List<T> list = DewPool.GetList(out handle);
		list.AddRange(enumerable);
		return list;
	}
}
