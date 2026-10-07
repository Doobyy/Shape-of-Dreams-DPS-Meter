using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_R_PhaseShift : AbilityInstance
{
	public DewCollider range;

	public ScalingValue dmgFactor;

	public ScalingValue secondDmgFactor;

	public float procCoefficient;

	public DewEase ease;

	public float shockDelay;

	public GameObject fxIndicator;

	public GameObject fxShockWave;

	public GameObject fxHit;

	public GameObject fxHitSound;

	public GameObject fxTeleport;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		FxPlayNewNetworked(fxTeleport, info.caster);
		FxPlayNewNetworked(fxTeleport, info.target);
		Vector3 teleportPos = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, info.target.agentPosition);
		Vector3 agentPosition = info.caster.agentPosition;
		if (info.caster.CheckEnemyOrNeutral(info.target))
		{
			CreateBasicEffect(info.target, new StunEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
			CreateDamage(DamageData.SourceType.Magic, dmgFactor).SetOriginPosition(teleportPos).Dispatch(info.target);
		}
		range.transform.position = teleportPos;
		if (!info.target.IsNullInactiveDeadOrKnockedOut() && !info.target.Status.hasCrowdControlImmunity)
		{
			Teleport(info.target, Dew.GetValidAgentPosition(agentPosition));
		}
		Teleport(info.caster, teleportPos);
		FxPlayNetworked(fxIndicator, teleportPos, Quaternion.identity);
		yield return new SI.WaitForSeconds(shockDelay);
		FxPlayNetworked(fxShockWave, teleportPos, Quaternion.identity);
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			if (i < 2)
			{
				FxPlayNewNetworked(fxHitSound, entity);
			}
			FxPlayNewNetworked(fxHit, entity);
			CreateDamage(DamageData.SourceType.Magic, secondDmgFactor, procCoefficient).SetOriginPosition(teleportPos).Dispatch(entity);
			if (!entity.Status.hasCrowdControlImmunity)
			{
				entity.Visual.KnockUp(KnockUpStrength.Normal, isFriendly: false);
				entity.Control.StartDisplacement(new DispByDestination
				{
					affectedByMovementSpeed = false,
					canGoOverTerrain = false,
					destination = Dew.GetValidAgentDestination_Closest(entity.agentPosition, teleportPos + Random.insideUnitCircle.ToXZ() * 0.5f),
					ease = ease,
					isCanceledByCC = false,
					isFriendly = false,
					duration = 0.75f
				});
			}
		}
		handle.Return();
		yield return new SI.WaitForSeconds(0.75f);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
