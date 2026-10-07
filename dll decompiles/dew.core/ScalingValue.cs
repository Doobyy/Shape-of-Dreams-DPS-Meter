using System;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public struct ScalingValue : ISerializationCallbackReceiver
{
	public const float SkillDefaultPerLevelMultiplier = 1.25f;

	public const float GemDefaultPerLevelMultiplier = 1.01f;

	public static int? levelOverride;

	public LevelScaling leveling;

	public float scalingMultiplier;

	public float baseValue;

	public float adFactor;

	public float apFactor;

	public float lvlFactor;

	public float armorFactor;

	public float addedHpFactor;

	public float critPercentageFactor;

	[JsonProperty]
	public string valueString
	{
		get
		{
			return ToString();
		}
		set
		{
			EditorParse(value, ref baseValue, ref adFactor, ref apFactor, ref lvlFactor, ref armorFactor, ref addedHpFactor, ref critPercentageFactor);
		}
	}

	public float GetValue(int level, Entity self)
	{
		if ((UnityEngine.Object)(object)self == null || (UnityEngine.Object)(object)self.Status == null)
		{
			return GetValue(level, 0f, 0f, 0f, 0f, 0f);
		}
		return GetValue(level, self.Status.attackDamage, self.Status.abilityPower, self.Status.armor, self.Status.GetBonusHealth(), self.Status.critChance);
	}

	public float GetValue(int level, float attackDamage, float abilityPower, float armor, float addedHp, float critChance)
	{
		if (levelOverride.HasValue)
		{
			level = levelOverride.Value;
		}
		level = Mathf.Max(level, 1);
		float num = GetScalingMultiplier(level);
		addedHp = Mathf.Max(addedHp, 0f);
		return (baseValue + lvlFactor * (float)level + adFactor * attackDamage + apFactor * abilityPower + armorFactor * armor + addedHpFactor * addedHp + critPercentageFactor * critChance * 100f) * num;
	}

	public float GetScalingMultiplier(int level)
	{
		if (leveling == LevelScaling.NoScaling)
		{
			return 1f;
		}
		return 1f + GetAddedScalingMultiplierPerLevel() * (float)Mathf.Max(0, level - 1);
	}

	public float GetAddedScalingMultiplierPerLevel()
	{
		if (leveling == LevelScaling.GemDefault)
		{
			return 0.00999999f * scalingMultiplier;
		}
		if (leveling == LevelScaling.SkillDefault)
		{
			return 0.25f * scalingMultiplier;
		}
		return 0f;
	}

	public ScalingValue(float baseVal, float ad, float ap, float lvl, float arm, float ahp, float critp, LevelScaling type)
	{
		baseValue = baseVal;
		adFactor = ad;
		apFactor = ap;
		lvlFactor = lvl;
		armorFactor = arm;
		addedHpFactor = ahp;
		critPercentageFactor = critp;
		leveling = type;
		scalingMultiplier = 1f;
	}

	public void OnBeforeSerialize()
	{
		if (scalingMultiplier <= 0.0001f)
		{
			scalingMultiplier = 1f;
		}
	}

	public void OnAfterDeserialize()
	{
	}

	public static implicit operator ScalingValue(string str)
	{
		ScalingValue result = new ScalingValue
		{
			scalingMultiplier = 1f
		};
		EditorParse(str, ref result.baseValue, ref result.adFactor, ref result.apFactor, ref result.lvlFactor, ref result.armorFactor, ref result.addedHpFactor, ref result.critPercentageFactor);
		return result;
	}

	public static explicit operator ScalingValue(float val)
	{
		return new ScalingValue
		{
			baseValue = val,
			scalingMultiplier = 1f
		};
	}

	public static ScalingValue operator *(ScalingValue sv, float multiplier)
	{
		ScalingValue result = new ScalingValue(sv.baseValue * multiplier, sv.adFactor * multiplier, sv.apFactor * multiplier, sv.lvlFactor * multiplier, sv.armorFactor * multiplier, sv.addedHpFactor * multiplier, sv.critPercentageFactor * multiplier, sv.leveling);
		result.scalingMultiplier = sv.scalingMultiplier;
		return result;
	}

	public static ScalingValue operator *(float multiplier, ScalingValue sv)
	{
		return sv * multiplier;
	}

	public static ScalingValue Lerp(ScalingValue a, ScalingValue b, float t)
	{
		ScalingValue result = new ScalingValue(Mathf.Lerp(a.baseValue, b.baseValue, t), Mathf.Lerp(a.adFactor, b.adFactor, t), Mathf.Lerp(a.apFactor, b.apFactor, t), Mathf.Lerp(a.lvlFactor, b.lvlFactor, t), Mathf.Lerp(a.armorFactor, b.armorFactor, t), Mathf.Lerp(a.addedHpFactor, b.addedHpFactor, t), Mathf.Lerp(a.critPercentageFactor, b.critPercentageFactor, t), a.leveling);
		result.scalingMultiplier = Mathf.Lerp(a.scalingMultiplier, b.scalingMultiplier, t);
		return result;
	}

	public override string ToString()
	{
		return EditorToString(baseValue, adFactor, apFactor, lvlFactor, armorFactor, addedHpFactor, critPercentageFactor);
	}

	public static string EditorToString(float baseVal, float ad, float ap, float lvl, float arm, float ahp, float critp)
	{
		string text = "";
		if (baseVal != 0f)
		{
			text = text + baseVal.ToString(CultureInfo.InvariantCulture) + " ";
		}
		if (ad != 0f)
		{
			text = text + ad.ToString(CultureInfo.InvariantCulture) + "ad ";
		}
		if (ap != 0f)
		{
			text = text + ap.ToString(CultureInfo.InvariantCulture) + "ap ";
		}
		if (lvl != 0f)
		{
			text = text + lvl.ToString(CultureInfo.InvariantCulture) + "x ";
		}
		if (arm != 0f)
		{
			text = text + arm.ToString(CultureInfo.InvariantCulture) + "arm ";
		}
		if (ahp != 0f)
		{
			text = text + ahp.ToString(CultureInfo.InvariantCulture) + "ahp ";
		}
		if (critp != 0f)
		{
			text = text + critp.ToString(CultureInfo.InvariantCulture) + "critp ";
		}
		if (!string.IsNullOrWhiteSpace(text))
		{
			return text.Trim();
		}
		return "0";
	}

	public static void EditorParse(string input, ref float baseVal, ref float ad, ref float ap, ref float lvl, ref float arm, ref float ahp, ref float critp)
	{
		baseVal = 0f;
		ad = 0f;
		ap = 0f;
		lvl = 0f;
		arm = 0f;
		ahp = 0f;
		critp = 0f;
		if (string.IsNullOrWhiteSpace(input))
		{
			return;
		}
		string[] array = input.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			try
			{
				if (text.EndsWith("ad", StringComparison.OrdinalIgnoreCase))
				{
					ad = float.Parse(text.Substring(0, text.Length - 2), CultureInfo.InvariantCulture);
				}
				else if (text.EndsWith("ap", StringComparison.OrdinalIgnoreCase))
				{
					ap = float.Parse(text.Substring(0, text.Length - 2), CultureInfo.InvariantCulture);
				}
				else if (text.EndsWith("x", StringComparison.OrdinalIgnoreCase))
				{
					lvl = float.Parse(text.Substring(0, text.Length - 1), CultureInfo.InvariantCulture);
				}
				else if (text.EndsWith("arm", StringComparison.OrdinalIgnoreCase))
				{
					arm = float.Parse(text.Substring(0, text.Length - 3), CultureInfo.InvariantCulture);
				}
				else if (text.EndsWith("ahp", StringComparison.OrdinalIgnoreCase))
				{
					ahp = float.Parse(text.Substring(0, text.Length - 3), CultureInfo.InvariantCulture);
				}
				else if (text.EndsWith("critp", StringComparison.OrdinalIgnoreCase))
				{
					critp = float.Parse(text.Substring(0, text.Length - 5), CultureInfo.InvariantCulture);
				}
				else
				{
					baseVal = float.Parse(text, CultureInfo.InvariantCulture);
				}
			}
			catch
			{
			}
		}
	}
}
