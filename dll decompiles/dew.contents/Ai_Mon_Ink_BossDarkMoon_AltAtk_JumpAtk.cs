using System;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_AltAtk_JumpAtk : InstantDamageInstance
{
	[NonSerialized]
	public bool isRage;

	[NonSerialized]
	public float scaleMultiplier = 1f;

	private Vector3 _baseMainEffectScale;

	private Vector3 _baseRangeScale;

	protected override void Awake()
	{
		base.Awake();
		if (mainEffectAfterDelay != null)
		{
			_baseMainEffectScale = mainEffectAfterDelay.transform.localScale;
		}
		if (range != null)
		{
			_baseRangeScale = range.transform.localScale;
		}
	}

	protected override void OnCreate()
	{
		if (mainEffectAfterDelay != null)
		{
			mainEffectAfterDelay.transform.localScale = (isRage ? (_baseMainEffectScale * scaleMultiplier) : _baseMainEffectScale);
		}
		if (range != null)
		{
			range.transform.localScale = (isRage ? (_baseRangeScale * scaleMultiplier) : _baseRangeScale);
		}
		base.OnCreate();
	}

	private void MirrorProcessed()
	{
	}
}
