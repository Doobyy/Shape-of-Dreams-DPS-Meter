using System;
using Mirror;
using UnityEngine;

public class LucidDream_SparklingDreamFlask : LucidDream
{
	public float potionAmount = 0.4f;

	public float shrineAmount = 1.2f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(OnActorAdd);
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			OnActorAdd(allActor);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer || !((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null))
		{
			return;
		}
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(OnActorAdd);
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			OnActorCleanup(allActor);
		}
	}

	private void OnActorAdd(Actor obj)
	{
		if (obj is Ai_RegenOrb_Projectile ai_RegenOrb_Projectile)
		{
			ai_RegenOrb_Projectile.actionOverride += (Action<Entity>)((Entity target) =>
			{
				if (target is Hero target2)
				{
					float value = NetworkedManagerBase<GameManager>.instance.GetSpecialRewardAmount_DreamDust() * UnityEngine.Random.Range(0.9f, 1.1f) * potionAmount;
					NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, DewMath.RandomRoundToInt(value), target.agentPosition, target2);
				}
			});
		}
		Shrine_Guidance shrine = obj as Shrine_Guidance;
		if (shrine == null)
		{
			return;
		}
		shrine.actionOverride += (Action<Entity>)((Entity target) =>
		{
			if (target is Hero target2)
			{
				float value = NetworkedManagerBase<GameManager>.instance.GetSpecialRewardAmount_DreamDust() * UnityEngine.Random.Range(0.9f, 1.1f) * shrineAmount;
				NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, DewMath.RandomRoundToInt(value), shrine.position, target2);
			}
		});
	}

	private void OnActorCleanup(Actor obj)
	{
		if (obj is Ai_RegenOrb_Projectile ai_RegenOrb_Projectile)
		{
			ai_RegenOrb_Projectile.actionOverride = null;
		}
		if (obj is Shrine_Guidance shrine_Guidance)
		{
			shrine_Guidance.actionOverride = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
