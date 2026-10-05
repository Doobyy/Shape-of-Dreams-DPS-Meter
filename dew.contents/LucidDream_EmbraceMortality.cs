using System;
using Mirror;
using UnityEngine;

public class LucidDream_EmbraceMortality : LucidDream
{
	public float damageAmp = 1f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd += new Action<Entity>(OnEntityAdd);
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			OnEntityAdd(allEntity);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer || !((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null))
		{
			return;
		}
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd -= new Action<Entity>(OnEntityAdd);
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			OnEntityCleanUp(allEntity);
		}
	}

	private void OnEntityAdd(Entity obj)
	{
		obj.dealtDamageProcessor.Add(Processor);
	}

	private void OnEntityCleanUp(Entity obj)
	{
		obj.dealtDamageProcessor.Remove(Processor);
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		Entity entity = actor.firstEntity;
		if (!((UnityEngine.Object)(object)entity == null) && !((UnityEngine.Object)(object)target == null) && !((UnityEngine.Object)(object)entity == (UnityEngine.Object)(object)target) && !data.IsAmountModifiedBy(this))
		{
			data.ApplyAmplification(damageAmp);
			data.SetAmountModifiedBy(this);
		}
	}

	private void MirrorProcessed()
	{
	}
}
