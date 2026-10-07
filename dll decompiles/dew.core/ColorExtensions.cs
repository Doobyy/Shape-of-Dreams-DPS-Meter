using UnityEngine;

public static class ColorExtensions
{
	public static Color WithR(this Color c, float value)
	{
		Color result = c;
		result.r = value;
		return result;
	}

	public static Color WithG(this Color c, float value)
	{
		Color result = c;
		result.g = value;
		return result;
	}

	public static Color WithB(this Color c, float value)
	{
		Color result = c;
		result.b = value;
		return result;
	}

	public static Color WithA(this Color c, float value)
	{
		Color result = c;
		result.a = value;
		return result;
	}

	public static float GetH(this Color c)
	{
		Color.RGBToHSV(c, out var H, out var _, out var _);
		return H;
	}

	public static float GetS(this Color c)
	{
		Color.RGBToHSV(c, out var _, out var S, out var _);
		return S;
	}

	public static float GetV(this Color c)
	{
		Color.RGBToHSV(c, out var _, out var _, out var V);
		return V;
	}

	public static void ToHSV(this Color c, out float h, out float s, out float v)
	{
		Color.RGBToHSV(c, out h, out s, out v);
	}

	public static Color WithH(this Color c, float value)
	{
		Color.RGBToHSV(c, out var _, out var S, out var V);
		return Color.HSVToRGB(Mathf.Repeat(value, 1f), S, V, hdr: true).WithA(c.a);
	}

	public static Color WithS(this Color c, float value)
	{
		Color.RGBToHSV(c, out var H, out var _, out var V);
		return Color.HSVToRGB(H, value, V, hdr: true).WithA(c.a);
	}

	public static Color WithV(this Color c, float value)
	{
		Color.RGBToHSV(c, out var H, out var S, out var _);
		return Color.HSVToRGB(H, S, value, hdr: true).WithA(c.a);
	}
}
