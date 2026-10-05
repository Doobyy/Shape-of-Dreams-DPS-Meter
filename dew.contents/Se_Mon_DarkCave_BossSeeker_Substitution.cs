using System;
using Mirror;
using UnityEngine;

public class Se_Mon_DarkCave_BossSeeker_Substitution : StatusEffect
{
	public float chance;

	public float dispDuration;

	public float dispAwayDistance;

	public DewEase dispEase;

	public GameObject fxAppearClone;

	public DewAnimationClip animAppear;

	public float cloneDestroyDamageThreshold = 0.25f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(OnTakeDamage);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(OnTakeDamage);
		}
	}

	private void OnTakeDamage(EventInfoDamage obj)
	{
		if (!(UnityEngine.Random.value >= chance))
		{
			Vector3 forward = ((Component)(object)victim).transform.forward;
			Vector3 agentPosition = victim.agentPosition;
			Quaternion value = victim.rotation;
			Vector3 center = SingletonBehaviour<DarkCave_BossRoomCenter>.instance.transform.position;
			Vector3 delta = center - agentPosition;
			CreateStatusEffect(victim, new CastInfo(info.caster), (Se_Mon_DarkCave_BossSeeker_Blink b) =>
			{
				b.customDestination = Dew.GetValidAgentDestination_Closest(victim.agentPosition, center + delta.normalized * delta.magnitude);
				b.duration = 1f;
			});
			Mon_DarkCave_SeekerHallucination mon_DarkCave_SeekerHallucination = Dew.SpawnEntity<Mon_DarkCave_SeekerHallucination>(agentPosition, value, this, DewPlayer.creep, info.caster.level);
			mon_DarkCave_SeekerHallucination.Status.SetHealth(victim.normalizedHealth * mon_DarkCave_SeekerHallucination.maxHealth);
			mon_DarkCave_SeekerHallucination.destroyHpThreshold = mon_DarkCave_SeekerHallucination.normalizedHealth - cloneDestroyDamageThreshold;
			mon_DarkCave_SeekerHallucination.Control.StartDaze(dispDuration + 1f);
			mon_DarkCave_SeekerHallucination.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = false,
				canGoOverTerrain = true,
				destination = Dew.GetValidAgentPosition(mon_DarkCave_SeekerHallucination.position + -forward * dispAwayDistance),
				duration = dispDuration,
				ease = dispEase,
				isCanceledByCC = false,
				isFriendly = true,
				rotateForward = false
			});
			FxPlayNewNetworked(fxAppearClone, mon_DarkCave_SeekerHallucination);
			mon_DarkCave_SeekerHallucination.Animation.PlayAbilityAnimation(animAppear);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
