using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_R_LightningDance : StatusEffect, ACH_THEYRE_JUST_BIG_CATS.ILightingActor
{
	public ScalingValue damage;

	public GameObject fxHit;

	public int bounceCount = 5;

	public float bounceInterval;

	public float targetRadius = 3f;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DoInvulnerable();
		DoUncollidable();
		victim.Control.freeMovement = true;
		victim.Visual.DisableRenderers();
		victim.Control.IncrementBlockCounters(Channel.BlockedAction.Dodge);
		Vector3 startPos = victim.agentPosition;
		yield return new SI.WaitForSeconds(bounceInterval * 0.7f);
		Entity target = info.target;
		for (int i = 0; i < bounceCount; i++)
		{
			if (target.IsNullInactiveDeadOrKnockedOut())
			{
				victim.Control.CancelOngoingDisplacement();
				Teleport(victim, Dew.GetValidAgentDestination_Closest(startPos, victim.agentPosition));
				Destroy();
				yield break;
			}
			victim.Control.StartDisplacement(new DispByDestination
			{
				destination = target.position + (target.position - victim.position).normalized * 0.4f,
				affectedByMovementSpeed = false,
				duration = bounceInterval,
				isFriendly = true,
				rotateForward = true,
				isCanceledByCC = false,
				canGoOverTerrain = true,
				ease = DewEase.EaseOutQuart
			});
			FxPlayNewNetworked(fxHit, target);
			Damage(damage).SetElemental(ElementalType.Light).Dispatch(target);
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, target.position, targetRadius, tvDefaultHarmfulEffectTargets).FilterInPlace((Entity e) =>
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Invalid comparison between Unknown and I4
				return (int)Dew.GetNavMeshPathStatus(e.agentPosition, startPos) == 0;
			});
			if (list.Count == 0)
			{
				List<Entity> list2 = DewPhysics.OverlapCircleAllEntities(out var handle2, target.position, targetRadius * 2f, tvDefaultHarmfulEffectTargets).FilterInPlace((Entity e) =>
				{
					//IL_000c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0012: Invalid comparison between Unknown and I4
					return (int)Dew.GetNavMeshPathStatus(e.agentPosition, startPos) == 0;
				});
				if (list2.Count == 0)
				{
					handle.Return();
					handle2.Return();
					victim.Control.CancelOngoingDisplacement();
					Teleport(victim, Dew.GetValidAgentDestination_Closest(startPos, victim.agentPosition));
					Destroy();
					yield break;
				}
				target = list2[Random.Range(0, list2.Count)];
				handle2.Return();
			}
			else
			{
				target = list[Random.Range(0, list.Count)];
			}
			handle.Return();
			yield return new SI.WaitForSeconds(bounceInterval);
		}
		victim.Control.CancelOngoingDisplacement();
		Teleport(victim, Dew.GetValidAgentDestination_Closest(startPos, victim.agentPosition));
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			victim.Visual.EnableRenderers();
			victim.Control.freeMovement = false;
			victim.Control.DecrementBlockCounters(Channel.BlockedAction.Dodge);
		}
	}

	private void MirrorProcessed()
	{
	}
}
