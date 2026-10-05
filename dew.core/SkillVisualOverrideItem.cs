using System;
using UnityEngine;

[Serializable]
public class SkillVisualOverrideItem
{
	public string[] targets = new string[0];

	public SkillMaterialReplacement[] materialReplacements = new SkillMaterialReplacement[0];

	public bool changeHue;

	[Range(0f, 360f)]
	public float hue;

	public bool changeSaturation;

	public float saturationMultiplier = 1f;

	public bool changeValue;

	public float valueMultiplier = 1f;

	public void Apply(GameObject gobj)
	{
		if (materialReplacements != null)
		{
			SkillMaterialReplacement[] array = materialReplacements;
			foreach (SkillMaterialReplacement skillMaterialReplacement in array)
			{
				if (skillMaterialReplacement != null)
				{
					DewEffect.ReplaceMaterialsRecursively(gobj, skillMaterialReplacement.from, skillMaterialReplacement.to);
				}
			}
		}
		DewEffect.ChangeColorRecursively(gobj, changeHue ? new float?(hue / 360f) : ((float?)null), changeSaturation ? saturationMultiplier : 1f, changeValue ? valueMultiplier : 1f);
	}
}
