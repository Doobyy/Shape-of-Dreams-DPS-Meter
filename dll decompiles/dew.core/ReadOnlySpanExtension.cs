using System;
using System.Runtime.CompilerServices;

public static class ReadOnlySpanExtension
{
	public unsafe static bool Contains<T>(this ReadOnlySpan<T> span, T item)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		ReadOnlySpan<T> val = span;
		for (int i = 0; i < val.Length; i++)
		{
			T val2 = System.Runtime.CompilerServices.Unsafe.Read<T>((void*)val[i]);
			if (val2.Equals(item))
			{
				return true;
			}
		}
		return false;
	}
}
