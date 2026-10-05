using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_Baam_Teleport : AbilityInstance
{
	public DewCollider Range;

	public AbilityTargetValidator hittable;

	public Knockback Knockback;

	public ScalingValue dmgFactor;

	public Vector3 targetPos;

	public DewEase ease;

	public GameObject tpStartEffect;

	public GameObject tpEndEffect;

	public GameObject hitEffect;

	public float appearDuration;

	public float finishDelay = 2f;

	public float runChance = 0.5f;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			FxPlayNewNetworked(tpStartEffect, info.caster);
			FxPlayNewNetworked(startEffect, info.caster);
			info.caster.Control.StartDaze(appearDuration);
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = false,
				canGoOverTerrain = true,
				destination = targetPos,
				duration = appearDuration,
				ease = ease,
				isCanceledByCC = false,
				isFriendly = true,
				onFinish = () =>
				{
					OnTeleportFinished();
					Destroy();
				},
				onCancel = Destroy,
				rotateForward = true
			});
		}
	}

	private void OnTeleportFinished()
	{
		FxPlayNetworked(tpEndEffect, info.caster);
		info.caster.Control.StartDaze(finishDelay);
		Range.transform.position = info.caster.position;
		Range.transform.rotation = info.caster.rotation;
		List<Entity> entities = Range.GetEntities(out var handle, hittable, info.caster);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetElemental(ElementalType.Light).Dispatch(entity);
			Knockback.ApplyWithOrigin(info.caster.position, entity);
			FxPlayNewNetworked(hitEffect, entity);
		}
		handle.Return();
		if (info.caster is Mon_Sky_Baam mon_Sky_Baam && Random.value < runChance)
		{
			mon_Sky_Baam.StartRunning();
		}
	}

	private void MirrorProcessed()
	{
	}
}
