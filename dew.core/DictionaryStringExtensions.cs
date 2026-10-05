using System;
using System.Collections.Generic;
using System.Text;

public static class DictionaryStringExtensions
{
	public static string ToPackedString(this IDictionary<string, string> dict, char pairSep = ',', char kvSep = '|')
	{
		if (dict == null || dict.Count == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder(dict.Count * 16);
		bool flag = true;
		foreach (KeyValuePair<string, string> item in dict)
		{
			if (!flag)
			{
				stringBuilder.Append(pairSep);
			}
			flag = false;
			stringBuilder.Append(ToB64(item.Key ?? string.Empty));
			stringBuilder.Append(kvSep);
			stringBuilder.Append(ToB64(item.Value ?? string.Empty));
		}
		return stringBuilder.ToString();
	}

	public static Dictionary<string, string> ToDictionaryFromPackedString(this string s, char pairSep = ',', char kvSep = '|')
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (string.IsNullOrEmpty(s))
		{
			return dictionary;
		}
		string[] array = s.Split(new char[1] { pairSep }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			int num = text.IndexOf(kvSep);
			if (num > 0)
			{
				string b = text.Substring(0, num);
				string b2 = text.Substring(num + 1);
				string key = FromB64(b);
				string value = FromB64(b2);
				dictionary[key] = value;
			}
		}
		return dictionary;
	}

	private static string ToB64(string str)
	{
		return Convert.ToBase64String(Encoding.UTF8.GetBytes(str ?? string.Empty));
	}

	private static string FromB64(string b64)
	{
		return Encoding.UTF8.GetString(Convert.FromBase64String(b64 ?? string.Empty));
	}
}
