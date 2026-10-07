using System;
using Mirror;
using UnityEngine;

public class LucidDream_GrievousWounds : LucidDream
{
	public float healMultiplier = 0.5f;

	public float shieldMultiplier = 0.5f;

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
		if (obj is Hero)
		{
			obj.takenHealProcessor.Add(HealProcessor, 200);
			obj.takenShieldProcessor.Add(ShieldProcessor);
		}
	}

	private void OnEntityCleanUp(Entity obj)
	{
		obj.takenHealProcessor.Remove(HealProcessor);
		obj.takenShieldProcessor.Remove(ShieldProcessor);
	}

	private void HealProcessor(ref HealData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			data.SetAmountModifiedBy(this);
			data.ApplyRawMultiplier(healMultiplier);
		}
	}

	private void ShieldProcessor(ref HealData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			data.SetAmountModifiedBy(this);
			data.ApplyRawMultiplier(shieldMultiplier);
		}
	}

	private void MirrorProcessed()
	{
	}
}
