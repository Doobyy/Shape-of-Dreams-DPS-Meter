using Mirror;
using UnityEngine;

public class Ai_Mon_LavaLand_BossInfernus_BreathFire_Projectile : StandardProjectile
{
	public class Ad_FlameJet
	{
		public float hitTime;
	}

	public float startHeight;

	public float procCoefficient = 0.4f;

	public float minHitInterval = 0.5f;

	public GameObject hitEffect;

	public ScalingValue damage;

	public Vector2 collisionRadiusRange;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		collisionRadius = collisionRadiusRange.x;
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		SetCustomStartPosition(info.caster.agentPosition + Vector3.up * startHeight + info.forward * startInFrontDistance);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			collisionRadius = Mathf.Lerp(collisionRadiusRange.x, collisionRadiusRange.y, normalizedPosition);
		}
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		if (hit.entity.TryGetData<Ad_FlameJet>(out var data))
		{
			if (Time.time - data.hitTime < minHitInterval)
			{
				return;
			}
			data.hitTime = Time.time;
		}
		else
		{
			hit.entity.AddData(new Ad_FlameJet
			{
				hitTime = Time.time
			});
		}
		FxPlayNewNetworked(hitEffect, hit.entity);
		Damage(damage, procCoefficient).SetElemental(ElementalType.Fire).SetAttr(DamageAttribute.DamageOverTime).SetOriginPosition(info.caster.position)
			.Dispatch(hit.entity);
	}

	private void MirrorProcessed()
	{
	}
}
