using System;
using UnityEngine;

public class Ai_R_FrozenFists_PunchDash : DashAttackInstance
{
	public ScalingValue shieldPerHit;

	public float shieldDuration = 3f;

	public float bossAmp = 2f;

	public float knockbackRandomMag = 0.5f;

	public float knockbackExtraDist = 1.5f;

	[NonSerialized]
	public bool disableKnockback;

	[NonSerialized]
	private float _baseKnockupAmount;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseKnockupAmount = knockupAmount;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		disableKnockback = false;
		knockupAmount = _baseKnockupAmount;
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		if (target.IsAnyBoss())
		{
			dmg.SetAttr(DamageAttribute.IsCrit);
			dmg.ApplyAmplification(bossAmp);
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		float num = (entity.IsAnyBoss() ? (bossAmp + 1f) : 1f);
		GiveShield(info.caster, GetValue(shieldPerHit) * num, shieldDuration, isDecay: true);
		if (!disableKnockback && currentDisplacement != null && !entity.Status.hasCrowdControlImmunity)
		{
			float num2 = Vector3.Dot((currentDisplacement.destination - entity.agentPosition).Flattened(), info.forward);
			if (num2 < 0f)
			{
				num2 = 0f;
			}
			Vector3 vector = entity.agentPosition + info.forward * (num2 + knockbackExtraDist) + UnityEngine.Random.insideUnitSphere * knockbackRandomMag;
			vector = Dew.GetPositionOnGround(vector);
			vector = Dew.GetValidAgentDestination_LinearSweep(entity.agentPosition, vector);
			entity.Control.StartDisplacement(new DispByDestination
			{
				destination = vector,
				ease = DewEase.EaseOutQuad,
				duration = dash.duration
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
