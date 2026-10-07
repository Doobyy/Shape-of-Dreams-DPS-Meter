using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossErebos_Teleport : AbilityInstance
{
	public float minCornerDistance;

	public float teleportDuration;

	public float endDelay;

	public float distance;

	public float searchEntityRange;

	public GameObject fxTeleport;

	public GameObject fxTelegraph;

	internal bool enableForceCenterDest;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			Vector3 dest = SingletonBehaviour<Erebos_BossRoomCenter>.instance.transform.position;
			if (!enableForceCenterDest)
			{
				dest = FindTeleportDestination();
			}
			info.caster.Visual.DisableRenderers();
			info.caster.Control.StartDaze(teleportDuration + endDelay + 0.5f);
			CreateBasicEffect(info.caster, new UncollidableEffect(), teleportDuration, "teleport_uncoll").DestroyOnDestroy(this);
			CreateBasicEffect(info.caster, new UnstoppableEffect(), teleportDuration, "teleport_unstop").DestroyOnDestroy(this);
			FxPlayNetworked(fxTeleport, dest + Vector3.up * 2.5f, null);
			Teleport(info.caster, dest);
			yield return new SI.WaitForSeconds(0.01f);
			yield return new SI.WaitForSeconds(teleportDuration);
			Hero closestAliveHero = Dew.GetClosestAliveHero(info.caster.agentPosition, fallbackToDead: true, info.caster);
			info.caster.AI.Aggro(closestAliveHero);
			FxPlayNetworked(fxTelegraph, dest, null);
			FxStopNetworked(fxTeleport);
			yield return new SI.WaitForSeconds(endDelay);
			CreateAbilityInstance<Ai_Mon_Special_BossErebos_Teleport_Instance>(dest, null, new CastInfo(info.caster, dest));
			info.caster.Visual.EnableRenderers();
			Destroy();
		}
	}

	private Vector3 FindTeleportDestination()
	{
		float num = 22f;
		Vector3 positionOnGround = Dew.GetPositionOnGround(SingletonBehaviour<Erebos_BossRoomCenter>.instance.transform.position);
		Vector3 result = positionOnGround + Random.insideUnitCircle.ToXZ() * Random.Range(1f, distance + 4f);
		Vector3 vector = positionOnGround + new Vector3(0f - num, 0f, num);
		Vector3 vector2 = positionOnGround + new Vector3(0f - num, 0f, 0f - num);
		Vector3 vector3 = positionOnGround + new Vector3(num, 0f, num);
		Vector3 vector4 = positionOnGround + new Vector3(num, 0f, 0f - num);
		bool flag = false;
		Vector3[] array = new Vector3[4] { vector, vector2, vector3, vector4 };
		foreach (Vector3 vector5 in array)
		{
			if (!((vector5 - info.caster.agentPosition).sqrMagnitude > minCornerDistance * minCornerDistance))
			{
				result = positionOnGround + (info.caster.agentPosition - vector5).normalized * Random.Range(0f, 5f) + Random.insideUnitCircle.ToXZ() * Random.Range(0f, distance);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.caster.agentPosition, searchEntityRange, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
			{
				includeUncollidable = true,
				sortComparer = CollisionCheckSettings.DistanceFromCenter
			});
			if (list.Count >= 1)
			{
				Entity entity = list[0];
				Vector3 normalized = (info.caster.agentPosition - entity.agentPosition).normalized;
				result = info.caster.agentPosition + normalized * distance;
				result = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, result);
				result = Dew.GetPositionOnGround(result);
			}
			handle.Return();
		}
		return result;
	}

	private void MirrorProcessed()
	{
	}
}
