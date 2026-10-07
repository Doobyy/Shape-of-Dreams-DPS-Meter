using System;
using Mirror;
using UnityEngine;

public class LucidDream_PrudentJellyfish : LucidDream
{
	public float cooldownFloorRatioByAbilityHaste = 0.4f;

	public float cooldownFloorRatioBySkillUpgrade = 0.4f;

	public float cooldownReductionStrength = 0.6f;

	protected override void OnCreate()
	{
		base.OnCreate();
		NetworkedManagerBase<GameManager>.instance.ges.cooldownFloorRatioByAbilityHaste = cooldownFloorRatioByAbilityHaste;
		NetworkedManagerBase<GameManager>.instance.ges.cooldownFloorRatioBySkillUpgrade = cooldownFloorRatioBySkillUpgrade;
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			HandleActor(allActor);
		}
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(HandleActor);
	}

	private void HandleActor(Actor a)
	{
		if (a is SkillTrigger skillTrigger)
		{
			skillTrigger.takenCooldownReductionProcessor.Add(Processor);
		}
	}

	private void Processor(ref CooldownReductionSettings data, Actor from, AbilityTrigger to)
	{
		data.amount *= cooldownReductionStrength;
	}

	private void CleanUpActor(Actor a)
	{
		if (a is SkillTrigger skillTrigger)
		{
			skillTrigger.takenCooldownReductionProcessor.Remove(Processor);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance != null)
		{
			NetworkedManagerBase<GameManager>.instance.ges.cooldownFloorRatioByAbilityHaste = 0f;
			NetworkedManagerBase<GameManager>.instance.ges.cooldownFloorRatioBySkillUpgrade = 0f;
		}
		if (!((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null))
		{
			return;
		}
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(HandleActor);
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			CleanUpActor(allActor);
		}
	}

	private void MirrorProcessed()
	{
	}
}
