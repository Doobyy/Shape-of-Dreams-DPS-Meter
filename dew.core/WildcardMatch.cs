public static class WildcardMatch
{
	public static bool EqualsWildcard(this string text, string wildcardString)
	{
		if (text == null || wildcardString == null)
		{
			return false;
		}
		int num = 0;
		int i = 0;
		int num2 = -1;
		int num3 = -1;
		while (num < text.Length)
		{
			if (i < wildcardString.Length && (wildcardString[i] == '?' || wildcardString[i] == text[num]))
			{
				num++;
				i++;
				continue;
			}
			if (i < wildcardString.Length && wildcardString[i] == '*')
			{
				num2 = i;
				num3 = num;
				i++;
				continue;
			}
			if (num2 != -1)
			{
				i = num2 + 1;
				num = ++num3;
				continue;
			}
			return false;
		}
		for (; i < wildcardString.Length && wildcardString[i] == '*'; i++)
		{
		}
		return i == wildcardString.Length;
	}

	public static bool EqualsWildcard(this string text, string wildcardString, bool ignoreCase)
	{
		if (ignoreCase)
		{
			return text.ToLower().EqualsWildcard(wildcardString.ToLower());
		}
		return text.EqualsWildcard(wildcardString);
	}
}
