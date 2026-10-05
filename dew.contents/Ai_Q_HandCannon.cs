using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Q_HandCannon : AbilityInstance
{
	public ScalingValue factor;

	public ScalingValue closeFactor;

	public DewCollider farRange;

	public DewCollider closeRange;

	public GameObject hitEffect;

	public bool enableKnockbackCloseRange;

	public Knockback knockback;

	public bool enableRecoil = true;

	public float recoilDuration = 0.5f;

	public float recoilDistance = 0.5f;

	public DewEase recoilEase = DewEase.EaseOutQuad;

	[NonSerialized]
	public bool alwaysKnockback;

	private ScalingValue _baseCloseFactor;

	private Vector3 _baseFarRangeScale;

	private Vector3 _baseCloseRangeScale;

	private Vector3 _baseStartEffectScale;

	private float _baseKnockbackDistance;

	private bool _baseEnableRecoil;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseCloseFactor = closeFactor;
		_baseFarRangeScale = farRange.transform.localScale;
		_baseCloseRangeScale = closeRange.transform.localScale;
		if (startEffect != null)
		{
			_baseStartEffectScale = startEffect.transform.localScale;
		}
		if (knockback != null)
		{
			_baseKnockbackDistance = knockback.distance;
		}
		_baseEnableRecoil = enableRecoil;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (enableRecoil)
		{
			DispByDestination disp = new DispByDestination
			{
				destination = info.caster.position - info.forward * recoilDistance,
				duration = recoilDuration,
				ease = recoilEase,
				isFriendly = true,
				rotateForward = false,
				canGoOverTerrain = false,
				isCanceledByCC = true
			};
			info.caster.Control.StartDisplacement(disp);
		}
		List<Entity> entities = closeRange.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		List<Entity> entities2 = farRange.GetEntities(out var handle2, tvDefaultHarmfulEffectTargets);
		foreach (Entity item in entities)
		{
			Damage(closeFactor).SetElemental(ElementalType.Fire).SetAttr(DamageAttribute.IsCrit).Dispatch(item);
			if (enableKnockbackCloseRange)
			{
				knockback.ApplyWithDirection(info.forward, item);
			}
			FxPlayNewNetworked(hitEffect, item);
		}
		foreach (Entity item2 in entities2)
		{
			if (!entities.Contains(item2))
			{
				Damage(factor).SetElemental(ElementalType.Fire).Dispatch(item2);
				if (alwaysKnockback)
				{
					knockback.ApplyWithDirection(info.forward, item2);
				}
				FxPlayNewNetworked(hitEffect, item2);
			}
		}
		handle.Return();
		handle2.Return();
		Destroy();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		alwaysKnockback = false;
		closeFactor = _baseCloseFactor;
		enableRecoil = _baseEnableRecoil;
		if (knockback != null)
		{
			knockback.distance = _baseKnockbackDistance;
		}
		farRange.transform.localScale = _baseFarRangeScale;
		closeRange.transform.localScale = _baseCloseRangeScale;
		if (startEffect != null)
		{
			startEffect.transform.localScale = _baseStartEffectScale;
		}
	}

	private void MirrorProcessed()
	{
	}
}
