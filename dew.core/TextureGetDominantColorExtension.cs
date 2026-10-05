using UnityEngine;

public static class TextureGetDominantColorExtension
{
	public static Color GetDominantColor(this Texture2D tex)
	{
		int num = 5;
		Color c = default;
		float num2 = float.NegativeInfinity;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				Color pixel = tex.GetPixel(Mathf.RoundToInt((float)tex.width * (float)(i + 1) / (float)(num + 2)), Mathf.RoundToInt((float)tex.width * (float)(j + 1) / (float)(num + 2)));
				if (!(pixel.a < 0.5f))
				{
					Color.RGBToHSV(pixel, out var _, out var S, out var V);
					float num3 = S * S * V * V * V;
					if (num3 > num2)
					{
						num2 = num3;
						c = pixel;
					}
				}
			}
		}
		return c.WithS(c.GetS() * 1.15f).WithA(1f);
	}

	public static Color GetBrightDominantColor(this Texture2D tex)
	{
		Color.RGBToHSV(tex.GetDominantColor(), out var H, out var S, out var V);
		S = (2f + S) / 3f;
		V = (2f + S) / 3f;
		return Color.HSVToRGB(H, S, V);
	}
}
