using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Mirror;
using UnityEngine;

public static class DewAdvText
{
	private static StringBuilder _sb = new StringBuilder();

	public static string Adv_ChatContent(DewPlayer player, string content)
	{
		return "<color=" + ChatManager.GetPlayerColorHex(player) + ">" + Adv_DescribedPlayerName(player) + ":</color> <color=#e4edf0>" + content + "</color>";
	}

	public static string Adv_ColoredDescribedPlayerName(DewPlayer player)
	{
		return $"@{{coloredDescribedPlayerName|{((NetworkBehaviour)player).netId}}}@";
	}

	public static string Adv_DescribedPlayerName(DewPlayer player)
	{
		return $"@{{describedPlayerName|{((NetworkBehaviour)player).netId}}}@";
	}

	public static string Adv_DescribedPlayerNameWithFallback(DewPlayer player)
	{
		string arg = (player.playerNameRaw ?? "").Replace("|", "").Replace("}@", "");
		return $"@{{describedPlayerNameWithFallback|{((NetworkBehaviour)player).netId}|{arg}}}@";
	}

	public static string Adv_ColoredPlayerName(DewPlayer player)
	{
		return $"@{{coloredPlayerName|{((NetworkBehaviour)player).netId}}}@";
	}

	public static string Adv_LocUI(string key)
	{
		return "@{ui|" + key + "}@";
	}

	public static string Adv_LocCurseName(string key)
	{
		return "@{curseName|" + key + "}@";
	}

	public static string Adv_LocCurseShortDesc(string key)
	{
		return "@{curseShortDesc|" + key + "}@";
	}

	public static string Adv_Format(string template, params string[] args)
	{
		_sb.Clear();
		_sb.Append("@{format|");
		_sb.Append(template);
		for (int i = 0; i < args.Length; i++)
		{
			_sb.Append("|");
			_sb.Append(args[i]);
		}
		_sb.Append("}@");
		return _sb.ToString();
	}

	public static string Adv_Number(float value, string format = "#,##0")
	{
		return "@{num|" + value.ToString(CultureInfo.InvariantCulture) + "|" + format + "}@";
	}

	public static string Adv_Cost(Cost cost)
	{
		if (cost.gold == 0 && cost.dreamDust == 0 && cost.stardust == 0 && cost.healthPercentage == 0 && cost.platinumCoin == 0)
		{
			return Adv_LocUI("Generic_Free_NoCost");
		}
		string text = "";
		if (cost.gold != 0)
		{
			if (text != "")
			{
				text += ", ";
			}
			text += Adv_Format(Adv_LocUI("Currency_Template_Gold"), Adv_Number(Mathf.Abs(cost.gold)));
		}
		if (cost.dreamDust != 0)
		{
			if (text != "")
			{
				text += ", ";
			}
			text += Adv_Format(Adv_LocUI("Currency_Template_DreamDust"), Adv_Number(Mathf.Abs(cost.dreamDust)));
		}
		if (cost.stardust != 0)
		{
			if (text != "")
			{
				text += ", ";
			}
			text += Adv_Format(Adv_LocUI("Currency_Template_Stardust"), Adv_Number(Mathf.Abs(cost.stardust)));
		}
		if (cost.healthPercentage != 0)
		{
			if (text != "")
			{
				text += ", ";
			}
			text += Adv_Format(Adv_LocUI("Currency_Template_HealthPercentage"), Adv_Number(Mathf.Abs(cost.healthPercentage)));
		}
		if (cost.platinumCoin != 0)
		{
			if (text != "")
			{
				text += ", ";
			}
			text += Adv_Format(Adv_LocUI("Currency_Template_PlatinumCoin"), Adv_Number(Mathf.Abs(cost.platinumCoin)));
		}
		return text;
	}

	public static string ConvertAdvToText(string adv)
	{
		if (string.IsNullOrEmpty(adv))
		{
			return adv;
		}
		if (adv.IndexOf("@{", StringComparison.InvariantCulture) == -1)
		{
			return adv;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		while (num < adv.Length)
		{
			int num2 = adv.IndexOf("@{", num, StringComparison.InvariantCulture);
			if (num2 == -1)
			{
				stringBuilder.Append(adv.Substring(num));
				break;
			}
			stringBuilder.Append(adv, num, num2 - num);
			int num3 = FindMatchingClosingTag(adv, num2);
			if (num3 == -1)
			{
				stringBuilder.Append(adv, num2, 2);
				num = num2 + 2;
			}
			else
			{
				string tagContent = adv.Substring(num2 + 2, num3 - (num2 + 2));
				stringBuilder.Append(ProcessTag(tagContent));
				num = num3 + 2;
			}
		}
		return stringBuilder.ToString();
	}

	private static string ProcessTag(string tagContent)
	{
		List<string> list = SplitByParameterDelimiter(tagContent);
		if (list.Count == 0)
		{
			return "@{}@";
		}
		List<string> list2 = new List<string>(list.Count);
		list2.Add(list[0]);
		for (int i = 1; i < list.Count; i++)
		{
			list2.Add(ConvertAdvToText(list[i]));
		}
		switch (list2[0])
		{
		case "format":
		{
			if (list2.Count < 2)
			{
				return "[Invalid format: " + tagContent + "]";
			}
			string text = list2[1];
			string[] array = list2.Skip(2).ToArray();
			try
			{
				object[] args = array;
				return string.Format(text, args);
			}
			catch (FormatException ex)
			{
				return "[Format Error: " + ex.Message + " in template '" + text + "']";
			}
		}
		case "num":
		{
			if (list2.Count < 2)
			{
				return "[Invalid num: " + tagContent + "]";
			}
			if (float.TryParse(list2[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
			{
				string format = DewLocalization.GetFormat((list2.Count > 2) ? list2[2] : "#,##0");
				return result.ToString(format, CultureInfo.CurrentCulture);
			}
			return "[Invalid number value: " + list2[1] + "]";
		}
		case "ui":
			return DewLocalization.GetUIValue(list2[1]);
		case "curseName":
			return DewLocalization.GetCurseName(list2[1]);
		case "curseShortDesc":
			return DewLocalization.ConvertDescriptionNodesToText(DewLocalization.GetCurseShortDescription(list2[1]), default);
		case "describedPlayerName":
		{
			uint netId4 = uint.Parse(list2[1]);
			DewPlayer dewPlayer4 = DewPlayer.allHumanPlayers.Find((DewPlayer pp) => ((NetworkBehaviour)pp).netId == netId4);
			if ((UnityEngine.Object)(object)dewPlayer4 == null)
			{
				return "???";
			}
			return ChatManager.GetDescribedPlayerName(dewPlayer4);
		}
		case "describedPlayerNameWithFallback":
		{
			if (list2.Count < 3)
			{
				return "???";
			}
			if (!uint.TryParse(list2[1], out var netId3))
			{
				return "???";
			}
			string text2 = list2[2];
			DewPlayer dewPlayer3 = DewPlayer.allHumanPlayers.Find((DewPlayer pp) => ((NetworkBehaviour)pp).netId == netId3);
			if ((UnityEngine.Object)(object)dewPlayer3 != null && !string.IsNullOrEmpty(dewPlayer3.playerName))
			{
				return ChatManager.GetDescribedPlayerName(dewPlayer3);
			}
			return DewSafety.FilterProfanityIfEnabled(text2);
		}
		case "coloredDescribedPlayerName":
		{
			uint netId2 = uint.Parse(list2[1]);
			DewPlayer dewPlayer2 = DewPlayer.allHumanPlayers.Find((DewPlayer pp) => ((NetworkBehaviour)pp).netId == netId2);
			if ((UnityEngine.Object)(object)dewPlayer2 == null)
			{
				return "???";
			}
			return ChatManager.GetColoredDescribedPlayerName(dewPlayer2);
		}
		case "coloredPlayerName":
		{
			uint netId = uint.Parse(list2[1]);
			DewPlayer dewPlayer = DewPlayer.allHumanPlayers.Find((DewPlayer pp) => ((NetworkBehaviour)pp).netId == netId);
			if ((UnityEngine.Object)(object)dewPlayer == null)
			{
				return "???";
			}
			return "<color=" + ChatManager.GetPlayerColorHex(dewPlayer) + ">" + dewPlayer.playerName + "</color>";
		}
		default:
			return "@{" + tagContent + "}@";
		}
	}

	private static int FindMatchingClosingTag(string text, int startIndex)
	{
		int num = 0;
		int num2 = startIndex;
		while (num2 < text.Length - 1)
		{
			if (text[num2] == '@' && text[num2 + 1] == '{')
			{
				num++;
				num2 += 2;
			}
			else if (text[num2] == '}' && text[num2 + 1] == '@')
			{
				num--;
				if (num == 0)
				{
					return num2;
				}
				num2 += 2;
			}
			else
			{
				num2++;
			}
		}
		return -1;
	}

	private static List<string> SplitByParameterDelimiter(string input)
	{
		List<string> list = new List<string>();
		if (input == null)
		{
			return list;
		}
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < input.Length; i++)
		{
			if (i + 1 < input.Length && input[i] == '@' && input[i + 1] == '{')
			{
				num++;
				i++;
			}
			else if (i + 1 < input.Length && input[i] == '}' && input[i + 1] == '@')
			{
				if (num > 0)
				{
					num--;
				}
				i++;
			}
			else if (input[i] == '|' && num == 0)
			{
				list.Add(input.Substring(num2, i - num2));
				num2 = i + 1;
			}
		}
		if (num2 <= input.Length)
		{
			list.Add(input.Substring(num2));
		}
		return list;
	}
}
