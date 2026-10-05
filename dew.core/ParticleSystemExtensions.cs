using UnityEngine;

public static class ParticleSystemExtensions
{
	public static void TintMainColor(this ParticleSystem ps, Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		MainModule main = ps.main;
		MinMaxGradient startColor = main.startColor;
		startColor.color *= color;
		main.startColor = startColor;
	}

	public static void SetMainColor(this ParticleSystem ps, Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		MainModule main = ps.main;
		MinMaxGradient startColor = main.startColor;
		startColor.color = color;
		main.startColor = startColor;
	}

	public static Color GetMainColor(this ParticleSystem ps)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		MainModule main = ps.main;
		MinMaxGradient startColor = main.startColor;
		return startColor.color;
	}
}
