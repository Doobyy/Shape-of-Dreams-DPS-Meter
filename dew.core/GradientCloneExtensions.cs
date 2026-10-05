using UnityEngine;

public static class GradientCloneExtensions
{
	public static Gradient Clone(this Gradient g)
	{
		if (g == null)
		{
			return null;
		}
		Gradient gradient = new Gradient
		{
			mode = g.mode
		};
		GradientColorKey[] colorKeys = g.colorKeys;
		gradient.colorKeys = colorKeys;
		gradient.alphaKeys = g.alphaKeys;
		gradient.colorSpace = g.colorSpace;
		return gradient;
	}

	public static MinMaxGradient Clone(this MinMaxGradient g)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		MinMaxGradient result = default;
		result.color = g.color;
		result.gradient = g.gradient.Clone();
		result.colorMax = g.colorMax;
		result.colorMin = g.colorMin;
		result.gradientMax = g.gradientMax.Clone();
		result.gradientMin = g.gradientMin.Clone();
		result.mode = g.mode;
		return result;
	}
}
