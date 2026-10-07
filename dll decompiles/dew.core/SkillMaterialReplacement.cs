using System;
using UnityEngine;

[Serializable]
public class SkillMaterialReplacement
{
	[Tooltip("Material to look for on the skill's renderers.")]
	public Material from;

	[Tooltip("Material to swap in wherever 'from' is found.")]
	public Material to;
}
