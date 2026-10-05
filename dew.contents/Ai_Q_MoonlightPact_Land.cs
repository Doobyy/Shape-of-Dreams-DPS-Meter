using System;
using UnityEngine;

public class Ai_Q_MoonlightPact_Land : InstantDamageInstance
{
	public float stunDuration = 0.5f;

	public ScalingValue healPerHitRatio;

	[NonSerialized]
	public float nachiaHealMultiplier;

	[NonSerialized]
	public Sum_Q_MoonlightPact_Fenrir fenrir;

	private Vector3 _baseRangeScale;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		if (range != null)
		{
			_baseRangeScale = range.transform.localScale;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		nachiaHealMultiplier = 0f;
		if (range != null)
		{
			range.transform.localScale = _baseRangeScale;
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new StunEffect(), stunDuration);
		if (!fenrir.IsNullInactiveDeadOrKnockedOut())
		{
			Heal(fenrir.maxHealth * GetValue(healPerHitRatio)).SetCanMerge().Dispatch(fenrir);
			if (nachiaHealMultiplier > 0f)
			{
				Heal(info.caster.maxHealth * GetValue(healPerHitRatio) * nachiaHealMultiplier).SetCanMerge().Dispatch(info.caster);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
