using System;
using UnityEngine;

public class Ai_L_MentalCorruption_Projectile : StandardProjectile
{
	public ScalingValue initDamageAmount;

	public float cooldownRefundRatio = 0.5f;

	public DewCollider range;

	[NonSerialized]
	public bool isSecondary;

	public float deviateMag = 3f;

	private Vector3 _deviateVector;

	private bool _didSpread;

	private Entity _refundCooldownTarget;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		if ((UnityEngine.Object)(object)_refundCooldownTarget != null)
		{
			_refundCooldownTarget.EntityEvent_OnDeath -= new Action<EventInfoKill>(RefundCooldown);
			_refundCooldownTarget = null;
		}
		isSecondary = false;
		_didSpread = false;
	}

	protected override void OnCreate()
	{
		if (!isSecondary)
		{
			position = info.caster.Visual.GetCenterPosition();
		}
		_deviateVector = UnityEngine.Random.insideUnitSphere * deviateMag;
		if (_deviateVector.y < 0f)
		{
			_deviateVector.y *= -1f;
		}
		base.OnCreate();
	}

	protected override Vector3 PositionSolver(float dt)
	{
		return base.PositionSolver(dt) + Mathf.Sin(normalizedPosition * (float)Math.PI) * _deviateVector;
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		_refundCooldownTarget = hit.entity;
		hit.entity.EntityEvent_OnDeath += new Action<EventInfoKill>(RefundCooldown);
		Damage(initDamageAmount).SetOriginPosition(info.caster.position).SetElemental(ElementalType.Dark).Dispatch(hit.entity);
		Se_L_MentalCorruption se_L_MentalCorruption = hit.entity.Status.FindStatusEffect((Se_L_MentalCorruption s) => (UnityEngine.Object)(object)s.info.caster == (UnityEngine.Object)(object)info.caster);
		if ((UnityEngine.Object)(object)se_L_MentalCorruption != null)
		{
			se_L_MentalCorruption.AddStack();
		}
		else
		{
			CreateStatusEffect<Se_L_MentalCorruption>(hit.entity, new CastInfo(info.caster));
		}
		if (!isSecondary && !_didSpread)
		{
			Spread();
		}
		Destroy();
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		if (!isSecondary && !_didSpread)
		{
			Spread();
		}
	}

	private void Spread()
	{
		_didSpread = true;
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			if (!((UnityEngine.Object)(object)entity == (UnityEngine.Object)(object)targetEntity))
			{
				CreateAbilityInstance(targetEntity.agentPosition, Quaternion.identity, new CastInfo(info.caster, entity), (Ai_L_MentalCorruption_Projectile p) =>
				{
					p.isSecondary = true;
				});
			}
		}
		handle.Return();
	}

	private void RefundCooldown(EventInfoKill obj)
	{
		AbilityTrigger abilityTrigger = firstTrigger;
		if (!((UnityEngine.Object)(object)abilityTrigger == null) && abilityTrigger.isActive && !((UnityEngine.Object)(object)abilityTrigger.owner == null) && abilityTrigger.owner.isActive)
		{
			ApplyCooldownReductionByRatio(abilityTrigger, cooldownRefundRatio);
		}
	}

	private void MirrorProcessed()
	{
	}
}
