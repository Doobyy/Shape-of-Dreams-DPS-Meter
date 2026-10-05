using UnityEngine;

public static class DewMath
{
	public static int RandomRoundToInt(float value, DewRandom random = null)
	{
		if (random == null)
		{
			random = DewRandom.instance;
		}
		if (float.IsPositiveInfinity(value))
		{
			return int.MaxValue;
		}
		if (float.IsNegativeInfinity(value))
		{
			return int.MinValue;
		}
		int num = Mathf.FloorToInt(value);
		if (random.Value() < value - (float)num)
		{
			num++;
		}
		return num;
	}

	public static float MultiplyPercentageBonuses(float a, float b)
	{
		return ((1f + a * 0.01f) * (1f + b * 0.01f) - 1f) * 100f;
	}

	public static float Lerp(this Vector2 vec, float t)
	{
		return Mathf.Lerp(vec.x, vec.y, t);
	}

	public static float RandomRange(this Vector2 vec)
	{
		return Random.Range(vec.x, vec.y);
	}
}
