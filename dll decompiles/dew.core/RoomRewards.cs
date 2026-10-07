using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

[RoomComponentStartDependency(typeof(RoomModifiers))]
[RoomComponentStartDependency(typeof(RoomMonsters))]
public class RoomRewards : RoomComponent
{
	private struct DroppedItem
	{
		public Type type;

		public int level;

		public Vector3 pos;
	}

	public bool disableRewards;

	[NonSerialized]
	public bool giveHighRarityReward;

	[NonSerialized]
	public int skillBonusLevel;

	[NonSerialized]
	public int gemBonusQuality;

	public bool isRegularRewardDisabled { get; private set; }

	[Server]
	public void DisableRegularRewards()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RoomRewards::DisableRegularRewards()' called when server was not active");
		}
		else
		{
			if (isRegularRewardDisabled)
			{
				return;
			}
			isRegularRewardDisabled = true;
			Actor[] array = NetworkedManagerBase<ActorManager>.instance.allActors.ToArray();
			foreach (Actor actor in array)
			{
				if ((UnityEngine.Object)(object)actor.FindFirstAncestorOfType<RoomModifierBase>() != null || !(actor is IProp prop))
				{
					continue;
				}
				if (actor is Shrine_Memory shrine_Memory && !giveHighRarityReward)
				{
					if (shrine_Memory.isAvailable)
					{
						shrine_Memory.Destroy();
						if (NetworkedManagerBase<ZoneManager>.instance._nextRewards != null)
						{
							NetworkedManagerBase<ZoneManager>.instance._nextRewards.Insert(0, RoomRewardFlowItemType.Skill);
						}
					}
				}
				else if (actor is Shrine_Concept shrine_Concept && !giveHighRarityReward)
				{
					if (shrine_Concept.isAvailable)
					{
						shrine_Concept.Destroy();
						if (NetworkedManagerBase<ZoneManager>.instance._nextRewards != null)
						{
							NetworkedManagerBase<ZoneManager>.instance._nextRewards.Insert(0, RoomRewardFlowItemType.Gem);
						}
					}
				}
				else if (prop.isRegularReward)
				{
					actor.Destroy();
				}
			}
		}
	}

	public override void OnRoomStartServer()
	{
		base.OnRoomStartServer();
		if (!isRevisit && !disableRewards && NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.Combat)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return null;
			SpawnCombatCoreShrines();
		}
	}

	[Server]
	public void SpawnCombatCoreShrines()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RoomRewards::SpawnCombatCoreShrines()' called when server was not active");
			return;
		}
		DewRandom roomRandom = room.GetRoomRandom(-641);
		if (giveHighRarityReward)
		{
			DewGameplayExperienceSettings ges = NetworkedManagerBase<GameManager>.instance.ges;
			float num = ges.combatRewardMemoryChance.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
			float num2 = ges.combatRewardFantasyChance.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
			float num3 = num + num2;
			if (roomRandom.Value() * num3 < num)
			{
				SpawnLockedShrineOnFinalSection<Shrine_Retrospection>(roomRandom);
			}
			else
			{
				SpawnLockedShrineOnFinalSection<Shrine_Enlightenment>(roomRandom);
			}
		}
		else
		{
			if (isRegularRewardDisabled)
			{
				return;
			}
			bool flag = false;
			if (NetworkedManagerBase<ZoneManager>.instance._nextRewards == null)
			{
				flag = true;
				NetworkedManagerBase<ZoneManager>.instance._nextRewards = new List<RoomRewardFlowItemType>();
			}
			if (NetworkedManagerBase<ZoneManager>.instance._nextRewards.Count == 0)
			{
				if (flag)
				{
					if (DewBuildProfile.current.customRewardFlowFirstTime != null && DewBuildProfile.current.customRewardFlowFirstTime.Length != 0)
					{
						NetworkedManagerBase<ZoneManager>.instance._nextRewards.AddRange(DewBuildProfile.current.customRewardFlowFirstTime);
					}
					else
					{
						NetworkedManagerBase<ZoneManager>.instance._nextRewards.Add(RoomRewardFlowItemType.Skill);
						NetworkedManagerBase<ZoneManager>.instance._nextRewards.Add(RoomRewardFlowItemType.Skill);
						NetworkedManagerBase<ZoneManager>.instance._nextRewards.Add(RoomRewardFlowItemType.None);
						NetworkedManagerBase<ZoneManager>.instance._nextRewards.Add(RoomRewardFlowItemType.None);
					}
					NetworkedManagerBase<ZoneManager>.instance._nextRewards.Shuffle(roomRandom);
				}
				else
				{
					if (DewBuildProfile.current.customRewardFlow != null && DewBuildProfile.current.customRewardFlow.Length != 0)
					{
						NetworkedManagerBase<ZoneManager>.instance._nextRewards.AddRange(DewBuildProfile.current.customRewardFlow);
					}
					else
					{
						NetworkedManagerBase<ZoneManager>.instance._nextRewards.Add(RoomRewardFlowItemType.Skill);
						NetworkedManagerBase<ZoneManager>.instance._nextRewards.Add(RoomRewardFlowItemType.Gem);
						if (NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex >= DewBuildProfile.current.luckShrineMinZoneIndex && roomRandom.Value() < DewBuildProfile.current.luckShrineInPoolChance)
						{
							NetworkedManagerBase<ZoneManager>.instance._nextRewards.Add(RoomRewardFlowItemType.Luck);
						}
						NetworkedManagerBase<ZoneManager>.instance._nextRewards.Add(RoomRewardFlowItemType.None);
						NetworkedManagerBase<ZoneManager>.instance._nextRewards.Add(RoomRewardFlowItemType.None);
					}
					NetworkedManagerBase<ZoneManager>.instance._nextRewards.Shuffle(roomRandom);
				}
			}
			RoomRewardFlowItemType roomRewardFlowItemType = NetworkedManagerBase<ZoneManager>.instance._nextRewards[0];
			NetworkedManagerBase<ZoneManager>.instance._nextRewards.RemoveAt(0);
			switch (roomRewardFlowItemType)
			{
			case RoomRewardFlowItemType.Skill:
				SpawnLockedShrineOnFinalSection<Shrine_Memory>(roomRandom);
				break;
			case RoomRewardFlowItemType.Gem:
				SpawnLockedShrineOnFinalSection<Shrine_Concept>(roomRandom);
				break;
			case RoomRewardFlowItemType.Any:
			{
				DewGameplayExperienceSettings ges2 = NetworkedManagerBase<GameManager>.instance.ges;
				float num4 = ges2.combatRewardMemoryChance.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
				ges2.combatRewardFantasyChance.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
				if (roomRandom.Value() < num4)
				{
					SpawnLockedShrineOnFinalSection<Shrine_Memory>(roomRandom);
				}
				else
				{
					SpawnLockedShrineOnFinalSection<Shrine_Concept>(roomRandom);
				}
				break;
			}
			case RoomRewardFlowItemType.Luck:
				SpawnLockedShrineOnFinalSection<Shrine_Luck>(roomRandom);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case RoomRewardFlowItemType.None:
				break;
			}
		}
	}

	private void SpawnLockedShrineOnFinalSection<T>(DewRandom random) where T : Shrine
	{
		SingletonDewNetworkBehaviour<Room>.instance.GetFinalSection().TryGetGoodNodePosition(out var position, random);
		T shrine = Dew.CreateActor<T>(position, null);
		shrine.isLocked = true;
		SingletonDewNetworkBehaviour<Room>.instance.onRoomClear.AddListener(() =>
		{
			if ((UnityEngine.Object)(object)shrine != null)
			{
				shrine.isLocked = false;
			}
		});
	}

	public void DropChaosReward(Vector3 pos, bool isHighQuality)
	{
		Dew.CreateActor(Dew.GetGoodRewardPosition(pos), null, null, (Shrine_Chaos chaos) =>
		{
			chaos.SetRandomRarity(isHighQuality);
		});
		foreach (DewPlayer p in DewPlayer.gamePlayers)
		{
			if (!p.hero.IsNullInactiveDeadOrKnockedOut() && !(room.GetRoomRandom((int)p.guid.GetStableHashCode()).Value() > p.doubleChaosChance))
			{
				Dew.CreateActor(Dew.GetGoodRewardPosition(pos), null, null, (Shrine_Chaos chaos) =>
				{
					chaos.SetRandomRarity(isHighQuality, SingletonDewNetworkBehaviour<Room>.instance.GetRoomRandom((int)p.guid.GetStableHashCode()));
					chaos.playersOverride = new string[1] { p.guid };
				});
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
