using System;
using Mirror;
using UnityEngine;

[SaveActor(true)]
public class Se_StarlessPath_PowerOfGuidance : StackedStatusEffect
{
	public float healMaxHpRatioOnTravel = 0.1f;

	public float healAmpPerStack = 0.1f;

	public GameObject fxHeal;

	public GameObject fxRiftOpenExplode;

	public int zoneCooldownAfterJonasVisit = 1;

	[SaveVar(SaveVarFlags.Default)]
	private bool _didOpenRift;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.takenHealProcessor.Add(TakenHealProcessor);
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}

	private void TakenHealProcessor(ref HealData data, Actor from, Entity to)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			data.ApplyAmplification(healAmpPerStack * (float)stack);
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		if (victim.IsNullInactiveDeadOrKnockedOut() || !obj.isTraveling || SingletonDewNetworkBehaviour<Room>.instance.isRevisit || NetworkedManagerBase<ZoneManager>.instance.currentZone.useSpecialGeneration)
		{
			return;
		}
		WorldNodeData currentNode = NetworkedManagerBase<ZoneManager>.instance.currentNode;
		if (currentNode.type != WorldNodeType.Combat || currentNode.HasMainModifier())
		{
			return;
		}
		GameMod_StarlessPath gameMod_StarlessPath = Dew.FindActorOfType<GameMod_StarlessPath>();
		if (!gameMod_StarlessPath.IsNullOrInactive() && gameMod_StarlessPath.lastJonasChamberVisitZoneIndex >= 0 && NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex <= gameMod_StarlessPath.lastJonasChamberVisitZoneIndex + zoneCooldownAfterJonasVisit)
		{
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
		{
			if (!victim.IsNullInactiveDeadOrKnockedOut())
			{
				FxPlayNewNetworked(fxHeal, victim);
				victim.Heal(healMaxHpRatioOnTravel * victim.maxHealth).Dispatch(victim);
			}
		});
		if (stack < maxStack || UnityEngine.Random.value > 0.6f)
		{
			return;
		}
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			if (allActor is Gem_U_GuidingCompass_NotCharged || allActor is Gem_U_GuidingCompass_Charged)
			{
				return;
			}
		}
		string item = "Rift_Sidetrack_TheChamberOfJonas";
		if (SingletonDewNetworkBehaviour<Room>.instance.rifts.openedSidetrackRifts.Contains(item))
		{
			return;
		}
		SingletonDewNetworkBehaviour<Room>.instance.rifts.openedSidetrackRifts.Add(item);
		SingletonDewNetworkBehaviour<Room>.instance.onRoomClear.AddListener(() =>
		{
			if (!this.IsNullOrInactive())
			{
				FxPlayNetworked(fxRiftOpenExplode, victim);
				ListReturnHandle<Se_StarlessPath_PowerOfGuidance> handle;
				foreach (Se_StarlessPath_PowerOfGuidance item2 in Dew.FindAllActorsOfType(out handle))
				{
					item2.Destroy();
				}
				handle.Return();
			}
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((bool)(UnityEngine.Object)(object)victim)
			{
				victim.takenHealProcessor.Remove(TakenHealProcessor);
			}
			if ((bool)(UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance)
			{
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
