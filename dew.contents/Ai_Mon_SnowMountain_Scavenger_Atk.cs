using UnityEngine;

public class Ai_Mon_SnowMountain_Scavenger_Atk : InstantDamageInstance
{
	private bool _baseScaleCached;

	private Vector3 _baseStartEffectScale;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		if (!_baseScaleCached)
		{
			_baseStartEffectScale = startEffectNoStop.transform.localScale;
			_baseScaleCached = true;
		}
		Vector3 baseStartEffectScale = _baseStartEffectScale;
		if (DewAnimationClip.GetEntryIndex(info.animSelectValue, 2) == 1)
		{
			baseStartEffectScale.x *= -1f;
		}
		startEffectNoStop.transform.localScale = baseStartEffectScale;
		base.OnCreate();
	}

	private void MirrorProcessed()
	{
	}
}
