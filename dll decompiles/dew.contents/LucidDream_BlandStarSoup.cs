using System;
using Mirror;

public class LucidDream_BlandStarSoup : LucidDream
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(OnActorAdd);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(OnActorAdd);
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			OnActorCleanup(allActor);
		}
	}

	private void OnActorAdd(Actor entity)
	{
		if (entity is Monster monster)
		{
			monster.dealtDamageProcessor.Add(TakeDamageExcluededElemental, 1000);
		}
	}

	private void OnActorCleanup(Actor entity)
	{
		if (entity is Monster monster)
		{
			monster.dealtDamageProcessor.Remove(TakeDamageExcluededElemental);
		}
	}

	private void TakeDamageExcluededElemental(ref DamageData data, Actor actor, Entity target)
	{
		data.SetElemental(null);
	}

	private void MirrorProcessed()
	{
	}
}
