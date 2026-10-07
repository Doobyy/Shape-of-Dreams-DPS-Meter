using System;
using UnityEngine;

namespace DewInternal;

public static class DewLocalizationNodeParser
{
	private static char[] ArithmeticExpressionChars = "+-*/()".ToCharArray();

	private static char[] FieldStartChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_".ToCharArray();

	public static void ParseBacktickedString(string inputText, Action<string> onNormalText, Action<string> onTag, Action<string> onBacktickedText)
	{
		int num = 0;
		int num2 = 0;
		int num3 = -1;
		int num4 = -1;
		while (true)
		{
			num++;
			if (num > 500)
			{
				throw new Exception("Max iteration limit passed");
			}
			if (num4 != -1)
			{
				int num5 = inputText.IndexOf('>', num2);
				if (num5 == -1)
				{
					throw new Exception("Unclosed Tag");
				}
				string obj = inputText.Substring(num4 + 1, num5 - num4 - 1);
				try
				{
					onTag?.Invoke(obj);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
				num2 = num5 + 1;
				num4 = -1;
				continue;
			}
			int num6 = inputText.IndexOf('`', num2);
			int num7 = inputText.IndexOf('<', num2);
			if (num6 != -1 && (num7 == -1 || num7 > num6))
			{
				if (num3 == -1)
				{
					num3 = num6;
					if (num2 != num3)
					{
						onNormalText?.Invoke(inputText.Substring(num2, num3 - num2));
					}
					num2 = num6 + 1;
					continue;
				}
				string obj2 = inputText.Substring(num3 + 1, num6 - num3 - 1);
				try
				{
					onBacktickedText?.Invoke(obj2);
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
				}
				num3 = -1;
				num2 = num6 + 1;
			}
			else
			{
				if (num7 == -1)
				{
					break;
				}
				num4 = num7;
				if (num2 != num7)
				{
					onNormalText?.Invoke(inputText.Substring(num2, num7 - num2));
				}
				num2 = num4 + 1;
			}
		}
		try
		{
			onNormalText?.Invoke(inputText.Substring(num2, inputText.Length - num2));
		}
		catch (Exception exception3)
		{
			Debug.LogException(exception3);
		}
	}

	public static void ParseExpression(string inputText, Action<string> onNormalExpression, Action<string> onField)
	{
		inputText = inputText.Replace(" ", "");
		int num = 0;
		while (num < inputText.Length)
		{
			int num2 = inputText.IndexOfAny(FieldStartChars, num);
			if (num2 != -1)
			{
				if (num != num2)
				{
					string obj = inputText.Substring(num, num2 - num);
					onNormalExpression?.Invoke(obj);
				}
				int num3 = inputText.IndexOfAny(ArithmeticExpressionChars, num2) - 1;
				if (num3 == -2)
				{
					num3 = inputText.Length - 1;
				}
				num = num3 + 1;
				string obj2 = inputText.Substring(num2, num3 - num2 + 1);
				onField?.Invoke(obj2);
				continue;
			}
			onNormalExpression?.Invoke(inputText.Substring(num, inputText.Length - num));
			break;
		}
	}
}
