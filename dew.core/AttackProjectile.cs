using System;
using UnityEngine;

public class AttackProjectile : StandardProjectile
{
	public float strength = 1f;

	[NonSerialized]
	public bool isCrit;

	[NonSerialized]
	public bool isMain = true;

	private ProjectileMode _baseMode;

	private float _baseEndDistance;

	private bool _baseCanCollideMidFlight;

	protected override bool destroyOnEntityHit => true;

	protected override void Awake()
	{
		base.Awake();
		_baseMode = mode;
		_baseEndDistance = endDistance;
		_baseCanCollideMidFlight = canCollideMidFlight;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Networkmode = _baseMode;
		NetworkendDistance = _baseEndDistance;
		canCollideMidFlight = _baseCanCollideMidFlight;
	}

	protected override void OnPrepare()
	{
		if (info.target == null)
		{
			Networkmode = ProjectileMode.Direction;
			canCollideMidFlight = true;
			NetworkendDistance = firstTrigger.currentConfig.effectiveRange;
		}
		base.OnPrepare();
	}

	protected override void OnCreate()
	{
		if (info.target == null)
		{
			Networkmode = ProjectileMode.Direction;
			canCollideMidFlight = true;
			AbilityTrigger abilityTrigger = firstTrigger;
			if ((UnityEngine.Object)(object)abilityTrigger != null && abilityTrigger.currentConfig != null)
			{
				NetworkendDistance = abilityTrigger.currentConfig.effectiveRange;
			}
		}
		base.OnCreate();
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		DoBasicAttackHit(firstEntity, hit.entity, isCrit, isMain, strength, strength);
		DestroyIfActive();
	}

	private void MirrorProcessed()
	{
	}
}
