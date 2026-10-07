using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Star_I_MoreShrineItems : StarEffect
{
	public StarScalingValue addChance;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(ClientEventOnActorAdd);
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			ClientEventOnActorAdd(allActor);
		}
	}

	private void ClientEventOnActorAdd(Actor obj)
	{
		if (obj is Shrine_Retrospection shrine_Retrospection)
		{
			if (UnityEngine.Random.value > GetValue(addChance))
			{
				shrine_Retrospection.persistentData.SetData("Se_Star_I_MoreShrineItems", "didFail", player.guid, value: true);
				return;
			}
			if (shrine_Retrospection.persistentData.GetDataOrDefault("Se_Star_I_MoreShrineItems", "didFail", player.guid, defaultValue: false))
			{
				return;
			}
			shrine_Retrospection.choiceOffset[player.guid] = CollectionExtensions.GetValueOrDefault<string, int>((IReadOnlyDictionary<string, int>)shrine_Retrospection.choiceOffset, player.guid, 0) + 1;
			if (((SyncIDictionary<string, ChoiceShrineItem[]>)(object)shrine_Retrospection.choices).ContainsKey(player.guid) && ((SyncIDictionary<string, ChoiceShrineItem[]>)(object)shrine_Retrospection.choices)[player.guid].Length < shrine_Retrospection.itemCount + shrine_Retrospection.choiceOffset[player.guid])
			{
				shrine_Retrospection.PopulatePlayerChoices(player);
			}
		}
		if (!(obj is Shrine_Enlightenment shrine_Enlightenment))
		{
			return;
		}
		if (UnityEngine.Random.value > GetValue(addChance))
		{
			shrine_Enlightenment.persistentData.SetData("Se_Star_I_MoreShrineItems", "didFail", player.guid, value: true);
		}
		else if (!shrine_Enlightenment.persistentData.GetDataOrDefault("Se_Star_I_MoreShrineItems", "didFail", player.guid, defaultValue: false))
		{
			shrine_Enlightenment.choiceOffset[player.guid] = CollectionExtensions.GetValueOrDefault<string, int>((IReadOnlyDictionary<string, int>)shrine_Enlightenment.choiceOffset, player.guid, 0) + 1;
			if (((SyncIDictionary<string, ChoiceShrineItem[]>)(object)shrine_Enlightenment.choices).ContainsKey(player.guid) && ((SyncIDictionary<string, ChoiceShrineItem[]>)(object)shrine_Enlightenment.choices)[player.guid].Length < shrine_Enlightenment.itemCount + shrine_Enlightenment.choiceOffset[player.guid])
			{
				shrine_Enlightenment.PopulatePlayerChoices(player);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance == null))
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(ClientEventOnActorAdd);
		}
	}

	private void MirrorProcessed()
	{
	}
}
