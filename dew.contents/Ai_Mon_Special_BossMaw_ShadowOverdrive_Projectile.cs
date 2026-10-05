using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossMaw_ShadowOverdrive_Projectile : StandardProjectile
{
	public float duration;

	public float range;

	public Knockback knockback;

	public ScalingValue dmgFactor;

	public GameObject fxTelegraph;

	public GameObject fxHit;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		initialSpeed = (info.point - info.caster.position).magnitude / duration;
		targetSpeed = initialSpeed;
		acceleration = 0f;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			float num = (info.point - info.caster.position).magnitude / initialSpeed;
			FxApplySpeedMultiplierNetworked(fxTelegraph, 1f / num);
			FxPlayNewNetworked(fxTelegraph, info.point, Quaternion.identity);
		}
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.point, range, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < list.Count; i++)
		{
			Entity entity = list[i];
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetDirection(info.point - info.caster.agentPosition).SetOriginPosition(info.point).SetElemental(ElementalType.Dark)
				.Dispatch(entity);
			knockback.ApplyWithOrigin(info.point, entity);
			FxPlayNewNetworked(fxHit, entity);
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
