using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_TeleportAtk : AbilityInstance
{
	public float duration;

	public float backDistance;

	public float delayAfterTeleport;

	public float postDelay = 0.55f;

	public DewEase ease;

	public bool doInvulnerable;

	public bool doUncollidable;

	public GameObject fxDisableRenderer;

	public GameObject fxTeleportStart;

	public GameObject fxTeleportEnd;

	public GameObject fxTelegraph;

	public DewAnimationClip spawnedStartClip;

	public GameObject fxDisableRendererSpawned;

	public GameObject fxSpawn;

	public GameObject fxFirstShown;

	public GameObject fxDespawn;

	private bool _isRage;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		_isRage = ((Mon_Ink_BossDarkMoon)info.caster)._isRage;
		Vector3 dir = (info.target.GetAIAgentPosition(info.caster) - info.caster.agentPosition).normalized;
		FxPlayNetworked(fxTeleportStart, info.caster.agentPosition, null);
		FxPlayNetworked(fxDisableRenderer, info.caster);
		info.caster.Control.StartDaze(duration + 0.5f);
		Vector3 targetPoint = info.target.GetAIAgentPosition(info.caster);
		Vector3 end = targetPoint + dir * backDistance;
		end = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, end);
		end = Dew.GetPositionOnGround(end);
		if (doInvulnerable)
		{
			CreateBasicEffect(info.caster, new InvulnerableEffect(), duration, "darkmoon_invul");
		}
		if (doUncollidable)
		{
			CreateBasicEffect(info.caster, new UncollidableEffect(), duration, "darkmoon_uncol");
		}
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			affectedByMovementSpeed = false,
			canGoOverTerrain = true,
			destination = end,
			duration = duration,
			ease = ease,
			isCanceledByCC = false,
			isFriendly = true,
			rotateForward = false,
			onCancel = DestroyIfActive,
			onFinish = () =>
			{
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
			}
		});
		if (!_isRage)
		{
			yield break;
		}
		yield return new SI.WaitForSeconds(duration);
		List<Entity> list = DewPool.GetList(out ListReturnHandle<Entity> handle);
		int num = 120;
		for (int num2 = 0; num2 < 2; num2++)
		{
			Vector3 vector = Quaternion.AngleAxis(num, Vector3.up) * dir;
			Vector3 vector2 = targetPoint - dir * 3f + vector * backDistance;
			vector2 = Dew.GetPositionOnGround(vector2);
			Mon_Ink_BossDarkMoonHallucination mon_Ink_BossDarkMoonHallucination = SpawnEntity(Vector3.zero, null, info.caster.owner, info.caster.level, (Mon_Ink_BossDarkMoonHallucination b) =>
			{
				b.Network_isSpecialAtk = true;
			});
			FxPlayNewNetworked(fxSpawn, mon_Ink_BossDarkMoonHallucination);
			DestroyOnDeath(info.caster);
			list.Add(mon_Ink_BossDarkMoonHallucination);
			num = -num;
			((MonoBehaviour)(object)this).StartCoroutine(Routine2(mon_Ink_BossDarkMoonHallucination, vector2));
		}
		handle.Return();
		yield return new SI.WaitForSeconds(duration + delayAfterTeleport + postDelay + 1f);
		DestroyIfActive();
		IEnumerator Routine()
		{
			FxStopNetworked(fxDisableRenderer);
			FxPlayNetworked(fxTeleportEnd, info.caster);
			Entity entity = info.target;
			if (entity.IsNullOrInactive())
			{
				entity = Dew.GetClosestAliveHero(info.caster.agentPosition, fallbackToDead: true, info.caster);
			}
			Quaternion rot = Quaternion.LookRotation(targetPoint - info.caster.agentPosition);
			info.caster.Control.RotateTowards(entity, immediately: true);
			info.caster.Control.StartDaze(delayAfterTeleport);
			FxPlayNewNetworked(fxTelegraph, info.caster.agentPosition, rot);
			yield return new WaitForSeconds(delayAfterTeleport);
			FxStopNetworked(fxTeleportEnd);
			info.caster.Control.StartDaze(postDelay);
			CreateAbilityInstance<Ai_Mon_Ink_BossDarkMoon_TeleportAtk_Instance>(info.caster.agentPosition, rot, new CastInfo(info.caster, CastInfo.GetAngle(rot)));
			if (!_isRage)
			{
				DestroyIfActive();
			}
		}
		IEnumerator Routine2(Entity e, Vector3 spawnPoint)
		{
			yield return new WaitForSeconds(duration);
			if (!e.IsNullOrInactive())
			{
				Teleport(e, spawnPoint);
				e.Control.Rotate(targetPoint - spawnPoint, immediately: true);
				Quaternion rot = Quaternion.LookRotation(targetPoint - e.agentPosition);
				FxPlayNewNetworked(fxFirstShown, e);
				FxPlayNewNetworked(fxTelegraph, e.agentPosition, rot);
				e.Animation.PlayAbilityAnimation(spawnedStartClip);
				yield return new WaitForSeconds(delayAfterTeleport);
				CreateAbilityInstance<Ai_Mon_Ink_BossDarkMoon_TeleportAtk_Instance>(e.agentPosition, rot, new CastInfo(e, CastInfo.GetAngle(rot)));
				FxPlayNewNetworked(fxDespawn, e);
				yield return new WaitForSeconds(postDelay);
				e.Destroy();
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxDisableRenderer);
		}
	}

	private void MirrorProcessed()
	{
	}
}
