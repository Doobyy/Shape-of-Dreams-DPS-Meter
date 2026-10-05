using System.Collections.Generic;
using System.Text;

public static class JoinArrayExtensions
{
	public static string JoinToString<T>(this IList<T> arr, string delimiter = " ")
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < arr.Count; i++)
		{
			T val = arr[i];
			stringBuilder.Append(val);
			if (i != arr.Count - 1)
			{
				stringBuilder.Append(delimiter);
			}
		}
		return stringBuilder.ToString();
	}
}
