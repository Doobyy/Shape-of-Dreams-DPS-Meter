using System;
using System.Collections.Generic;

public static class StringExtensions
{
	public static List<string> SplitToList(this string s, string delimiter = " ")
	{
		if (string.IsNullOrEmpty(s))
		{
			return new List<string>();
		}
		if (delimiter == null)
		{
			throw new ArgumentNullException("delimiter");
		}
		if (delimiter.Length == 0)
		{
			return new List<string> { s };
		}
		return new List<string>(s.Split(new string[1] { delimiter }, StringSplitOptions.None));
	}

	public static List<string> SplitToList(this string s, char delimiter)
	{
		if (string.IsNullOrEmpty(s))
		{
			return new List<string>();
		}
		return new List<string>(s.Split(delimiter, StringSplitOptions.None));
	}
}
