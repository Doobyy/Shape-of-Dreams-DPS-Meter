using System;
using UnityEngine;

public class Ai_Mon_Special_BossPolaris_Holy_CounterSpell_SkillProjectile : StandardProjectile
{
	[NonSerialized]
	public Color tintColor;

	protected override void OnCreate()
	{
		DewEffect.TintRecursively(((Component)(object)this).gameObject, tintColor);
		base.OnCreate();
	}

	private void MirrorProcessed()
	{
	}
}
