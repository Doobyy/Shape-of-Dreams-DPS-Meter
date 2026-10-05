using System;
using UnityEngine;

[Serializable]
public struct StarScalingValue
{
	public enum StrengthScalingType
	{
		Multiply = 0,
		Divide = 1,
		NormalizedBiggerBetter = 10,
		NormalizedSmallerBetter = 11
	}

	public float v0;

	public float v1;

	public float v2;

	public float v3;

	public float v4;

	public float v5;

	public float v6;

	public float v7;

	public bool isInteger;

	public StrengthScalingType strengthScaling;

	public bool isClamped;

	public float clampMin;

	public float clampMax;

	[HideInInspector]
	public int levels;

	[HideInInspector]
	public bool isParentStarEffect;

	public float GetValue(int level, float strength)
	{
		if (ScalingValue.levelOverride.HasValue)
		{
			level = ScalingValue.levelOverride.Value;
		}
		float num = Get(level - 1);
		if (!Mathf.Approximately(strength, 1f))
		{
			num = strengthScaling switch
			{
				StrengthScalingType.Multiply => num * strength, 
				StrengthScalingType.Divide => num / strength, 
				StrengthScalingType.NormalizedBiggerBetter => (!(strength > 1f)) ? (num * strength) : Mathf.Min(1f - (1f - num) / strength, num * strength), 
				StrengthScalingType.NormalizedSmallerBetter => (!(strength > 1f)) ? Mathf.Min(1f - (1f - num) * strength, num / strength) : (num / strength), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		if (isInteger)
		{
			if (Mathf.Approximately(strength, 1f))
			{
				num = Mathf.RoundToInt(num);
			}
			else
			{
				num = ((strength > 1f != (strengthScaling == StrengthScalingType.Multiply)) ? ((float)Mathf.FloorToInt(num)) : ((float)Mathf.RoundToInt(num)));
			}
		}
		if (isClamped)
		{
			num = Mathf.Clamp(num, clampMin, clampMax);
		}
		return num;
	}

	public float Get(int index)
	{
		index = ((!isParentStarEffect) ? Mathf.Clamp(index, 0, 7) : Mathf.Clamp(index, 0, levels - 1));
		return index switch
		{
			0 => v0, 
			1 => v1, 
			2 => v2, 
			3 => v3, 
			4 => v4, 
			5 => v5, 
			6 => v6, 
			7 => v7, 
			_ => 0f, 
		};
	}

	public void Set(int index, float value)
	{
		index = Mathf.Clamp(index, 0, levels - 1);
		switch (index)
		{
		case 0:
			v0 = value;
			break;
		case 1:
			v1 = value;
			break;
		case 2:
			v2 = value;
			break;
		case 3:
			v3 = value;
			break;
		case 4:
			v4 = value;
			break;
		case 5:
			v5 = value;
			break;
		case 6:
			v6 = value;
			break;
		case 7:
			v7 = value;
			break;
		}
	}
}
