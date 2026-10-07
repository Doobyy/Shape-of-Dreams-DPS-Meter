using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class StatisticsManager : ManagerBase<StatisticsManager>
{
	public bool collectStatistics = true;

	[SaveVar(SaveVarFlags.Default)]
	private List<string> _incrementedPlayCount = new List<string>();

	private void Start()
	{
		if (!collectStatistics)
		{
			return;
		}
		StartCoroutine(PlayTimeCollectRoutine());
		Dew.CallOnReady(this, () => (UnityEngine.Object)(object)DewPlayer.local != null, () =>
		{
			DewPlayer.local.ClientEvent_OnHeroChanged += new Action<Hero, Hero>(CheckHero);
			DewPlayer.local.ClientEvent_OnSpendGold += (Action<int>)((int amount) =>
			{
				if (!((UnityEngine.Object)(object)DewPlayer.local.hero == null) && DewSave.profileStats.heroes.TryGetValue(((object)DewPlayer.local.hero).GetType().Name, out var value))
				{
					value.spentGold += amount;
				}
			});
			DewPlayer.local.ClientEvent_OnSpendDreamDust += (Action<int>)((int amount) =>
			{
				if (!((UnityEngine.Object)(object)DewPlayer.local.hero == null) && DewSave.profileStats.heroes.TryGetValue(((object)DewPlayer.local.hero).GetType().Name, out var value))
				{
					value.spentDreamDust += amount;
				}
			});
			CheckHero(null, DewPlayer.local.hero);
		});
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += (Action<EventInfoLoadRoom>)((EventInfoLoadRoom room) =>
		{
			if (!((UnityEngine.Object)(object)DewPlayer.local.hero == null) && !room.isLoadingFromSave)
			{
				if (DewSave.profileStats.heroes.TryGetValue(((object)DewPlayer.local.hero).GetType().Name, out var value))
				{
					value.visitedLocations++;
					if (NetworkedManagerBase<ZoneManager>.instance.isCurrentNodeHunted)
					{
						value.visitedHunterLocations++;
					}
				}
				if (NetworkedManagerBase<ZoneManager>.instance.currentZone != null && DewSave.profileStats.zones.TryGetValue(NetworkedManagerBase<ZoneManager>.instance.currentZone.name, out var value2))
				{
					value2.visitedLocations++;
					if (NetworkedManagerBase<ZoneManager>.instance.isCurrentNodeHunted)
					{
						value2.visitedHunterLocations++;
					}
				}
			}
		});
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded += (Action<EventInfoLoadZone>)((EventInfoLoadZone zone) =>
		{
			if (!((UnityEngine.Object)(object)DewPlayer.local.hero == null) && zone.isTraveling)
			{
				if (DewSave.profileStats.heroes.TryGetValue(((object)DewPlayer.local.hero).GetType().Name, out var value))
				{
					value.visitedWorlds++;
				}
				if (NetworkedManagerBase<ZoneManager>.instance.currentZone != null && DewSave.profileStats.zones.TryGetValue(NetworkedManagerBase<ZoneManager>.instance.currentZone.name, out var value2))
				{
					value2.visited++;
				}
			}
		});
		NetworkedManagerBase<ClientEventManager>.instance.OnChaosUsed += (Action<DewPlayer>)((DewPlayer player) =>
		{
			if (((NetworkBehaviour)player).isLocalPlayer && !((UnityEngine.Object)(object)DewPlayer.local.hero == null) && DewSave.profileStats.heroes.TryGetValue(((object)DewPlayer.local.hero).GetType().Name, out var value))
			{
				value.chaosCount++;
			}
		});
		NetworkedManagerBase<ClientEventManager>.instance.OnItemUpgraded += (Action<Hero, NetworkBehaviour>)((Hero hero, NetworkBehaviour target) =>
		{
			if (((NetworkBehaviour)hero).isOwned)
			{
				if (DewSave.profileStats.heroes.TryGetValue(((object)hero).GetType().Name, out var value))
				{
					value.upgradeCount++;
				}
				DewProfileStats.ItemData value3;
				if (((object)target).GetType().IsSubclassOf(typeof(SkillTrigger)) && DewSave.profileStats.skills.TryGetValue(((object)target).GetType().Name, out var value2))
				{
					value2.upgradeCount++;
				}
				else if (((object)target).GetType().IsSubclassOf(typeof(Gem)) && DewSave.profileStats.gems.TryGetValue(((object)target).GetType().Name, out value3))
				{
					value3.upgradeCount++;
				}
			}
		});
		NetworkedManagerBase<ClientEventManager>.instance.OnDismantled += (Action<Hero, NetworkBehaviour>)((Hero hero, NetworkBehaviour target) =>
		{
			if (((NetworkBehaviour)hero).isOwned)
			{
				if (DewSave.profileStats.heroes.TryGetValue(((object)hero).GetType().Name, out var value))
				{
					value.dismantleCount++;
				}
				DewProfileStats.ItemData value3;
				if (((object)target).GetType().IsSubclassOf(typeof(SkillTrigger)) && DewSave.profileStats.skills.TryGetValue(((object)target).GetType().Name, out var value2))
				{
					value2.dismantleCount++;
				}
				else if (((object)target).GetType().IsSubclassOf(typeof(Gem)) && DewSave.profileStats.gems.TryGetValue(((object)target).GetType().Name, out value3))
				{
					value3.dismantleCount++;
				}
			}
		});
		NetworkedManagerBase<ClientEventManager>.instance.OnItemBought += (Action<Hero, NetworkBehaviour>)((Hero hero, NetworkBehaviour target) =>
		{
			if (((NetworkBehaviour)hero).isOwned)
			{
				if (DewSave.profileStats.heroes.TryGetValue(((object)hero).GetType().Name, out var value))
				{
					value.buyCount++;
				}
				DewProfileStats.ItemData value3;
				if (((object)target).GetType().IsSubclassOf(typeof(SkillTrigger)) && DewSave.profileStats.skills.TryGetValue(((object)target).GetType().Name, out var value2))
				{
					value2.buyCount++;
				}
				else if (((object)target).GetType().IsSubclassOf(typeof(Gem)) && DewSave.profileStats.gems.TryGetValue(((object)target).GetType().Name, out value3))
				{
					value3.buyCount++;
				}
			}
		});
		NetworkedManagerBase<ClientEventManager>.instance.OnItemSold += (Action<Hero, NetworkBehaviour>)((Hero hero, NetworkBehaviour target) =>
		{
			if (((NetworkBehaviour)hero).isOwned)
			{
				if (DewSave.profileStats.heroes.TryGetValue(((object)hero).GetType().Name, out var value))
				{
					value.sellCount++;
				}
				DewProfileStats.ItemData value3;
				if (((object)target).GetType().IsSubclassOf(typeof(SkillTrigger)) && DewSave.profileStats.skills.TryGetValue(((object)target).GetType().Name, out var value2))
				{
					value2.sellCount++;
				}
				else if (((object)target).GetType().IsSubclassOf(typeof(Gem)) && DewSave.profileStats.gems.TryGetValue(((object)target).GetType().Name, out value3))
				{
					value3.sellCount++;
				}
			}
		});
		void CheckHero(Hero from, Hero to)
		{
			if (!((UnityEngine.Object)(object)to == null))
			{
				DewPlayer.local.hero.Skill.ClientHeroEvent_OnSkillPickup += (Action<SkillTrigger>)((SkillTrigger obj) =>
				{
					IncrementPlayCount(((object)obj).GetType());
				});
				DewPlayer.local.hero.Skill.ClientHeroEvent_OnGemPickup += (Action<Gem>)((Gem obj) =>
				{
					IncrementPlayCount(((object)obj).GetType());
				});
				DewPlayer.local.hero.ClientHeroEvent_OnKillOrAssist += (Action<EventInfoKill>)((EventInfoKill obj) =>
				{
					if (obj.victim is Monster monster)
					{
						if (DewSave.profileStats.monsters.TryGetValue(((object)monster).GetType().Name, out var value))
						{
							value.kills++;
							if (NetworkedManagerBase<GameManager>.instance.difficulty.name == "diffNightmare" || NetworkedManagerBase<GameManager>.instance.difficulty.name == "diffLimbo")
							{
								value.nightmareKills++;
							}
						}
						if (NetworkedManagerBase<ZoneManager>.instance.currentZone != null && DewSave.profileStats.zones.TryGetValue(NetworkedManagerBase<ZoneManager>.instance.currentZone.name, out var value2))
						{
							value2.kills++;
							if (monster.isHunter)
							{
								value2.hunterKills++;
							}
							if (monster.type == Monster.MonsterType.Boss)
							{
								value2.heroicBossKills++;
							}
							if (monster.type == Monster.MonsterType.MiniBoss)
							{
								value2.miniBossKills++;
							}
						}
					}
				});
				DewPlayer.local.hero.ClientHeroEvent_OnKnockedOut += (Action<EventInfoKill>)((EventInfoKill obj) =>
				{
					if ((UnityEngine.Object)(object)obj.actor != null && obj.actor.firstEntity is Monster monster && DewSave.profileStats.monsters.TryGetValue(((object)monster).GetType().Name, out var value))
					{
						value.deaths++;
					}
					if (NetworkedManagerBase<ZoneManager>.instance.currentZone != null && DewSave.profileStats.zones.TryGetValue(NetworkedManagerBase<ZoneManager>.instance.currentZone.name, out var value2))
					{
						value2.deaths++;
					}
				});
				IncrementPlayCount(((object)to).GetType());
			}
		}
		static IEnumerator PlayTimeCollectRoutine()
		{
			while (true)
			{
				yield return new WaitForSeconds(6f);
				try
				{
					if (!((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.softInstance == null) && !NetworkedManagerBase<GameManager>.instance.isGameTimePaused && !((UnityEngine.Object)(object)DewPlayer.local == null) && !DewPlayer.local.hero.IsNullOrInactive())
					{
						if (DewSave.profileStats.heroes.TryGetValue(((object)DewPlayer.local.hero).GetType().Name, out var value))
						{
							value.playTimeMinutes += 0.10000000149011612;
						}
						if (DewSave.profileStats.zones.TryGetValue(NetworkedManagerBase<ZoneManager>.instance.currentZone.name, out var value2))
						{
							value2.playTimeMinutes += 0.10000000149011612;
						}
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
	}

	private void IncrementPlayCount(Type target)
	{
		if (_incrementedPlayCount.Contains(target.Name))
		{
			return;
		}
		if (target.IsSubclassOf(typeof(SkillTrigger)))
		{
			if (DewSave.profileStats.skills.TryGetValue(target.Name, out var value))
			{
				value.playCount++;
			}
		}
		else if (target.IsSubclassOf(typeof(Gem)))
		{
			if (DewSave.profileStats.gems.TryGetValue(target.Name, out var value2))
			{
				value2.playCount++;
			}
		}
		else
		{
			if (!target.IsSubclassOf(typeof(Hero)))
			{
				return;
			}
			if (DewSave.profileStats.heroes.TryGetValue(target.Name, out var value3))
			{
				value3.playCount++;
			}
		}
		_incrementedPlayCount.Add(target.Name);
	}
}
