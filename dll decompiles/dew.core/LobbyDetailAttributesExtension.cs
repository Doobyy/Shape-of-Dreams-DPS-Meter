using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using Mirror;

public static class LobbyDetailAttributesExtension
{
	public static AttributeDataValue ToAttrDataValue(this object o)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		AttributeDataValue result;
		if (o is string text)
		{
			result = default;
			result.AsUtf8 = Utf8String.op_Implicit(text);
			return result;
		}
		if (o is double value)
		{
			result = default;
			result.AsDouble = value;
			return result;
		}
		if (o is float num)
		{
			result = default;
			result.AsDouble = num;
			return result;
		}
		if (o is int num2)
		{
			result = default;
			result.AsInt64 = num2;
			return result;
		}
		if (o is long value2)
		{
			result = default;
			result.AsInt64 = value2;
			return result;
		}
		if (o is bool value3)
		{
			result = default;
			result.AsBool = value3;
			return result;
		}
		if (o is SyncList<string> arr)
		{
			result = default;
			result.AsUtf8 = Utf8String.op_Implicit(((IList<string>)arr).JoinToString(","));
			return result;
		}
		if (o is List<string> arr2)
		{
			result = default;
			result.AsUtf8 = Utf8String.op_Implicit(arr2.JoinToString(","));
			return result;
		}
		if (o is Dictionary<string, string> dict)
		{
			result = default;
			result.AsUtf8 = Utf8String.op_Implicit(dict.ToPackedString());
			return result;
		}
		if (o is AllowMidJoinType allowMidJoinType)
		{
			result = default;
			result.AsInt64 = (long)allowMidJoinType;
			return result;
		}
		throw new ArgumentException("o");
	}

	public static string GetAttributeString(this LobbyDetails lobby, string key)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		LobbyDetailsCopyAttributeByKeyOptions val = default;
		val.AttrKey = Utf8String.op_Implicit(key);
		LobbyDetailsCopyAttributeByKeyOptions val2 = val;
		Attribute? val3 = default;
		if ((int)lobby.CopyAttributeByKey(ref val2, ref val3) == 0 && val3.HasValue)
		{
			Attribute value = val3.Value;
			if (value.Data.HasValue)
			{
				value = val3.Value;
				AttributeData value2 = value.Data.Value;
				AttributeDataValue value3 = value2.Value;
				return Utf8String.op_Implicit(value3.AsUtf8);
			}
		}
		return "";
	}

	public static List<string> GetAttributeStringList(this LobbyDetails lobby, string key)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		LobbyDetailsCopyAttributeByKeyOptions val = default;
		val.AttrKey = Utf8String.op_Implicit(key);
		LobbyDetailsCopyAttributeByKeyOptions val2 = val;
		Attribute? val3 = default;
		if ((int)lobby.CopyAttributeByKey(ref val2, ref val3) == 0 && val3.HasValue)
		{
			Attribute value = val3.Value;
			if (value.Data.HasValue)
			{
				value = val3.Value;
				AttributeData value2 = value.Data.Value;
				AttributeDataValue value3 = value2.Value;
				return ((object)value3.AsUtf8).ToString().SplitToList(",");
			}
		}
		return new List<string>();
	}

	public static Dictionary<string, string> GetAttributeStringDict(this LobbyDetails lobby, string key)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		LobbyDetailsCopyAttributeByKeyOptions val = default;
		val.AttrKey = Utf8String.op_Implicit(key);
		LobbyDetailsCopyAttributeByKeyOptions val2 = val;
		Attribute? val3 = default;
		if ((int)lobby.CopyAttributeByKey(ref val2, ref val3) == 0 && val3.HasValue)
		{
			Attribute value = val3.Value;
			if (value.Data.HasValue)
			{
				value = val3.Value;
				AttributeData value2 = value.Data.Value;
				AttributeDataValue value3 = value2.Value;
				return ((object)value3.AsUtf8).ToString().ToDictionaryFromPackedString();
			}
		}
		return new Dictionary<string, string>();
	}

	public static string GetMemberAttributeString(this LobbyDetails lobby, ProductUserId member, string key)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		LobbyDetailsCopyMemberAttributeByKeyOptions val = default;
		val.TargetUserId = member;
		val.AttrKey = Utf8String.op_Implicit(key);
		LobbyDetailsCopyMemberAttributeByKeyOptions val2 = val;
		Attribute? val3 = default;
		if ((int)lobby.CopyMemberAttributeByKey(ref val2, ref val3) == 0 && val3.HasValue)
		{
			Attribute value = val3.Value;
			if (value.Data.HasValue)
			{
				value = val3.Value;
				AttributeData value2 = value.Data.Value;
				AttributeDataValue value3 = value2.Value;
				return Utf8String.op_Implicit(value3.AsUtf8);
			}
		}
		return "";
	}

	public static long GetAttributeLong(this LobbyDetails lobby, string key)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		LobbyDetailsCopyAttributeByKeyOptions val = default;
		val.AttrKey = Utf8String.op_Implicit(key);
		LobbyDetailsCopyAttributeByKeyOptions val2 = val;
		Attribute? val3 = default;
		if ((int)lobby.CopyAttributeByKey(ref val2, ref val3) == 0 && val3.HasValue)
		{
			Attribute value = val3.Value;
			if (value.Data.HasValue)
			{
				value = val3.Value;
				AttributeData value2 = value.Data.Value;
				AttributeDataValue value3 = value2.Value;
				return value3.AsInt64.GetValueOrDefault();
			}
		}
		return 0L;
	}

	public static AllowMidJoinType GetAttributeMidJoin(this LobbyDetails lobby, string key)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		LobbyDetailsCopyAttributeByKeyOptions val = default;
		val.AttrKey = Utf8String.op_Implicit(key);
		LobbyDetailsCopyAttributeByKeyOptions val2 = val;
		Attribute? val3 = default;
		if ((int)lobby.CopyAttributeByKey(ref val2, ref val3) == 0 && val3.HasValue)
		{
			Attribute value = val3.Value;
			if (value.Data.HasValue)
			{
				value = val3.Value;
				AttributeData value2 = value.Data.Value;
				AttributeDataValue value3 = value2.Value;
				return (AllowMidJoinType)value3.AsInt64.GetValueOrDefault();
			}
		}
		return AllowMidJoinType.Disallow;
	}

	public static bool GetAttributeBool(this LobbyDetails lobby, string key)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		LobbyDetailsCopyAttributeByKeyOptions val = default;
		val.AttrKey = Utf8String.op_Implicit(key);
		LobbyDetailsCopyAttributeByKeyOptions val2 = val;
		Attribute? val3 = default;
		if ((int)lobby.CopyAttributeByKey(ref val2, ref val3) == 0 && val3.HasValue)
		{
			Attribute value = val3.Value;
			if (value.Data.HasValue)
			{
				value = val3.Value;
				AttributeData value2 = value.Data.Value;
				AttributeDataValue value3 = value2.Value;
				return value3.AsBool == true;
			}
		}
		return false;
	}
}
