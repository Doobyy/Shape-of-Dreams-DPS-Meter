using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public static class DewSafety
{
	private static HashSet<string> _wholeWordProfanities;

	private static HashSet<string> _substringProfanities;

	private static Regex _wholeWordRegex;

	private static Regex _substringRegex;

	private static bool _isInitialized;

	private static void InitializeProfanityFilter()
	{
		if (!_isInitialized)
		{
			_wholeWordProfanities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			TextAsset[] array = Resources.LoadAll<TextAsset>("Profanities/WholeWord");
			for (int i = 0; i < array.Length; i++)
			{
				LoadWordsFromTextAsset(array[i], _wholeWordProfanities);
			}
			_substringProfanities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			array = Resources.LoadAll<TextAsset>("Profanities/Substring");
			for (int i = 0; i < array.Length; i++)
			{
				LoadWordsFromTextAsset(array[i], _substringProfanities);
			}
			BuildOptimizedRegexPatterns();
			_isInitialized = true;
			Debug.Log($"Loaded {_wholeWordProfanities.Count} whole word profanities and {_substringProfanities.Count} substring profanities");
		}
	}

	private static void BuildOptimizedRegexPatterns()
	{
		if (_wholeWordProfanities.Count > 0)
		{
			List<string> list = new List<string>(_wholeWordProfanities);
			list.Sort((string a, string b) => b.Length.CompareTo(a.Length));
			StringBuilder stringBuilder = new StringBuilder("(?<!\\p{L})(");
			for (int num = 0; num < list.Count; num++)
			{
				if (num > 0)
				{
					stringBuilder.Append("|");
				}
				stringBuilder.Append(Regex.Escape(list[num]));
			}
			stringBuilder.Append(")(?!\\p{L})");
			_wholeWordRegex = new Regex(stringBuilder.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled);
		}
		if (_substringProfanities.Count <= 0)
		{
			return;
		}
		List<string> list2 = new List<string>(_substringProfanities);
		list2.Sort((string a, string b) => b.Length.CompareTo(a.Length));
		StringBuilder stringBuilder2 = new StringBuilder("(");
		for (int num2 = 0; num2 < list2.Count; num2++)
		{
			if (num2 > 0)
			{
				stringBuilder2.Append("|");
			}
			stringBuilder2.Append(Regex.Escape(list2[num2]));
		}
		stringBuilder2.Append(")");
		_substringRegex = new Regex(stringBuilder2.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled);
	}

	private static void LoadWordsFromTextAsset(TextAsset textAsset, HashSet<string> targetSet)
	{
		if (!(textAsset != null) || string.IsNullOrEmpty(textAsset.text))
		{
			return;
		}
		string[] array = textAsset.text.Split(new char[2] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim();
			if (!string.IsNullOrEmpty(text))
			{
				targetSet.Add(text);
			}
		}
	}

	public static string FilterProfanityIfEnabled(string text)
	{
		if (DewSave.profileMain.gameplay.enableProfanityFilter)
		{
			return FilterProfanity(text);
		}
		return text;
	}

	public static string FilterProfanity(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		InitializeProfanityFilter();
		string text2 = text;
		if (_wholeWordRegex != null)
		{
			text2 = _wholeWordRegex.Replace(text2, (Match match) => new string('*', match.Value.Length));
		}
		if (_substringRegex != null)
		{
			text2 = _substringRegex.Replace(text2, (Match match) => new string('*', match.Value.Length));
		}
		return text2;
	}
}
