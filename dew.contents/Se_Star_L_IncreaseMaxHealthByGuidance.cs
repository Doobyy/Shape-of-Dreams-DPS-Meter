using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Star_L_IncreaseMaxHealthByGuidance : StarEffect
{
	public StarScalingValue defaultIncreaseMaxHealth;

	public StarScalingValue blessedIncreaseMaxHealth;

	[SaveVar(SaveVarFlags.Default)]
	private StatBonus _bonus;

	private readonly List<(Shrine shrine, Action<Entity> handler)> _useSubscriptions = new List<(Shrine, Action<Entity>)>();

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(ClientEvent_OnActorAdd);
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			ClientEvent_OnActorAdd(allActor);
		}
		_bonus = DoStatBonus(_bonus);
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
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(ClientEvent_OnActorAdd);
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

	private void ClientEvent_OnActorAdd(Actor obj)
	{
		if (obj is Shrine_BlessedGuidance shrine_BlessedGuidance)
		{
			Action<Entity> action = (Entity _) =>
			{
				_bonus.maxHealthFlat += GetValue(blessedIncreaseMaxHealth);
			};
			shrine_BlessedGuidance.ClientEvent_OnSuccessfulUse += action;
			_useSubscriptions.Add((shrine_BlessedGuidance, action));
		}
		else if (obj is Shrine_Guidance shrine_Guidance)
		{
			Action<Entity> action2 = (Entity _) =>
			{
				_bonus.maxHealthFlat += GetValue(defaultIncreaseMaxHealth);
			};
			shrine_Guidance.ClientEvent_OnSuccessfulUse += action2;
			_useSubscriptions.Add((shrine_Guidance, action2));
		}
	}

	private void MirrorProcessed()
	{
	}
}
