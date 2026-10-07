using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossErebos_StarRain_Instance : StandardProjectile
{
	public ScalingValue dmgFactor;

	public DewCollider range;

	public Knockback Knockback;

	public GameObject fxHit;

	public GameObject fxTelegraph;

	public GameObject fxExplosion;

	internal float duration;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		Vector3 vector = info.caster.Visual.GetWeaponPosition() + new Vector3(0f, 0.5f, 0f);
		SetCustomStartPosition(vector);
		initialSpeed = (targetPosition - vector).magnitude / duration;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			FxPlayNewNetworked(fxTelegraph, targetPosition, Quaternion.identity);
		}
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		FxPlayNetworked(fxExplosion, targetPosition, Quaternion.identity);
		range.transform.position = targetPosition;
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			FxPlayNewNetworked(fxHit, entity);
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(targetPosition).Dispatch(entity);
			Knockback.ApplyWithOrigin(targetPosition, entity);
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
