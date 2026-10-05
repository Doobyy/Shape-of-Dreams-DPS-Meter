using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Star_I_ExtraChaosOptionGoldOnUse : StarEffect
{
	public int addedChoices = 1;

	public StarScalingValue goldOnChaos;

	private readonly List<(Shrine shrine, Action<Entity> handler)> _useSubscriptions = new List<(Shrine, Action<Entity>)>();

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(ActorAdd);
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			ActorAdd(allActor);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((bool)(UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(ActorAdd);
		}
		foreach (var (shrine, action) in _useSubscriptions)
		{
			if ((UnityEngine.Object)(object)shrine != null)
			{
				shrine.ClientEvent_OnSuccessfulUse -= action;
			}
		}
		_useSubscriptions.Clear();
	}

	private void ActorAdd(Actor obj)
	{
		Shrine_Chaos ch = obj as Shrine_Chaos;
		if (ch != null)
		{
			ch.choiceOffset[player.guid] = CollectionExtensions.GetValueOrDefault<string, int>((IReadOnlyDictionary<string, int>)ch.choiceOffset, player.guid, 0) + addedChoices;
			Action<Entity> action = (Entity entity) =>
			{
				OnSuccessfulUse(entity, ch.position);
			};
			ch.ClientEvent_OnSuccessfulUse += action;
			_useSubscriptions.Add((ch, action));
		}
		Shrine_CorruptedChaos cc = obj as Shrine_CorruptedChaos;
		if (cc != null)
		{
			cc.choiceOffset[player.guid] = CollectionExtensions.GetValueOrDefault<string, int>((IReadOnlyDictionary<string, int>)cc.choiceOffset, player.guid, 0) + addedChoices;
			Action<Entity> action2 = (Entity entity) =>
			{
				OnSuccessfulUse(entity, cc.position);
			};
			cc.ClientEvent_OnSuccessfulUse += action2;
			_useSubscriptions.Add((cc, action2));
		}
	}

	private void OnSuccessfulUse(Entity obj, Vector3 dropGoldPos)
	{
		if (!((UnityEngine.Object)(object)obj != (UnityEngine.Object)(object)hero))
		{
			NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, GetValueInt(goldOnChaos), dropGoldPos);
		}
	}

	private void MirrorProcessed()
	{
	}
}
