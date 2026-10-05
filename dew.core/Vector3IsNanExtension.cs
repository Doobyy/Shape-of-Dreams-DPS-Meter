using UnityEngine;

public static class Vector3IsNanExtension
{
	public static bool IsNaN(this Vector4 v)
	{
		if (!float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z))
		{
			return float.IsNaN(v.w);
		}
		return true;
	}

	public static bool IsNaN(this Quaternion v)
	{
		if (!float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z))
		{
			return float.IsNaN(v.w);
		}
		return true;
	}

	public static bool IsNaN(this Vector3 v)
	{
		if (!float.IsNaN(v.x) && !float.IsNaN(v.y))
		{
			return float.IsNaN(v.z);
		}
		return true;
	}

	public static bool IsNaN(this Vector2 v)
	{
		if (!float.IsNaN(v.x))
		{
			return float.IsNaN(v.y);
		}
		return true;
	}
}
