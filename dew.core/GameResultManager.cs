using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class GameResultManager : NetworkedManagerBase<GameResultManager>
{
	private class Ad_RegisteredTracker
	{
	}

	public DewGameResult current;

	public SafeAction<DewGameResult> ClientEvent_OnGameConcluded;

	public SafeAction<DewGameResult> onUpdateGameResult;

	private long _startTimestamp;

	private string _localPlayerGuid;

	[SaveVar(SaveVarFlags.Default)]
	public DewGameResult tracked { get; private set; }

	public override void OnStartServer()
	{
		base.OnStartServer();
		GameManager.CallOnReady(() =>
		{
			if (tracked == null)
			{
				StartTrackingLazy();
			}
		});
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += (Action<EventInfoLoadRoom>)((EventInfoLoadRoom obj) =>
		{
			if (!obj.isLoadingFromSave)
			{
				GameManager.CallOnReady(UpdateAndSendMidGameResultToClients);
			}
		});
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += (Action<EventInfoLoadRoom>)((EventInfoLoadRoom obj) =>
		{
			if (tracked != null && obj.isTraveling)
			{
				tracked.visitedLocations++;
			}
		});
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded += (Action<EventInfoLoadZone>)((EventInfoLoadZone obj) =>
		{
			if (tracked != null && obj.isTraveling)
			{
				tracked.visitedWorlds++;
			}
		});
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnHeroAdd += (Action<Hero>)((Hero h) =>
		{
			StartTrackingPlayer(h.owner);
		});
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		_startTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
		Dew.CallOnReady((MonoBehaviour)(object)this, () => (UnityEngine.Object)(object)DewPlayer.local != null && !string.IsNullOrEmpty(DewPlayer.local.guid), () =>
		{
			_localPlayerGuid = DewPlayer.local.guid;
		});
		UnityEngine.Object.FindObjectOfType<UI_CreditsView>(includeInactive: true).onEnd += (Action)(() =>
		{
			InGameUIManager.instance.SetState("Result");
		});
	}

	private void StartTrackingLazy()
	{
		tracked = new DewGameResult
		{
			result = DewGameResult.ResultType.Conceded
		};
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			StartTrackingPlayer(gamePlayer);
		}
		Debug.Log("Started tracking game statistics");
		RpcRegisterResult(tracked, didGameEnd: false);
	}

	[Server]
	public void WrapUp(DewGameResult.ResultType type)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameResultManager::WrapUp(DewGameResult/ResultType)' called when server was not active");
			return;
		}
		if (tracked == null)
		{
			Debug.LogWarning("No game result to wrap up");
			return;
		}
		UpdateGameResult();
		DewGameResult dewGameResult = tracked;
		tracked = null;
		dewGameResult.result = type;
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			gamePlayer.isReady = false;
		}
		RpcRegisterResult(dewGameResult, didGameEnd: true);
	}

	[Server]
	public void UpdateAndSendMidGameResultToClients()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameResultManager::UpdateAndSendMidGameResultToClients()' called when server was not active");
			return;
		}
		if (tracked == null)
		{
			Debug.LogWarning("No game result to update and send");
			return;
		}
		UpdateGameResult();
		RpcRegisterResult(tracked, didGameEnd: false);
	}

	[Server]
	private void UpdateGameResult()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameResultManager::UpdateGameResult()' called when server was not active");
			return;
		}
		foreach (DewGameResult.PlayerData player2 in tracked.players)
		{
			DewPlayer player = player2.GetPlayer();
			if ((UnityEngine.Object)(object)player == null)
			{
				continue;
			}
			Hero hero = player.hero;
			if (hero.IsNullOrInactive())
			{
				continue;
			}
			player2.loadout = hero.loadout;
			player2.skills.Clear();
			player2.gems.Clear();
			player2.maxGemCounts.Clear();
			foreach (KeyValuePair<int, AbilityTrigger> ability in hero.Ability.abilities)
			{
				if (ability.Value is SkillTrigger skill && ability.Key >= 0 && ability.Key <= 4)
				{
					player2.skills.Add(new DewGameResult.SkillData((HeroSkillLocation)ability.Key, skill));
				}
			}
			foreach (KeyValuePair<GemLocation, Gem> gem in hero.Skill.gems)
			{
				player2.gems.Add(new DewGameResult.GemData(gem.Key, gem.Value));
			}
			for (HeroSkillLocation heroSkillLocation = HeroSkillLocation.Q; heroSkillLocation <= HeroSkillLocation.Movement; heroSkillLocation++)
			{
				player2.maxGemCounts.Add(hero.Skill.GetMaxGemCount(heroSkillLocation));
			}
			player2.maxHealth = hero.maxHealth;
			player2.attackDamage = hero.Status.attackDamage;
			player2.abilityPower = hero.Status.abilityPower;
			player2.skillHaste = hero.Status.abilityHaste;
			player2.attackSpeed = 1f / hero.Ability.attackAbility.configs[0].cooldownTime * hero.Status.attackSpeedMultiplier;
			player2.fireAmp = hero.Status.fireEffectAmp + 1f;
			player2.armor = hero.Status.armor;
			player2.addedHp = hero.Status.GetBonusHealth();
			player2.critChance = hero.Status.critChance;
		}
		tracked.elapsedGameTimeSeconds = (int)NetworkedManagerBase<GameManager>.instance.elapsedGameTime;
		tracked.difficulty = NetworkedManagerBase<GameManager>.instance.difficulty.name;
		onUpdateGameResult?.Invoke(tracked);
	}

	[ClientRpc]
	private void RpcRegisterResult(DewGameResult result, bool didGameEnd)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_DewGameResult((NetworkWriter)(object)val, result);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, didGameEnd);
		((NetworkBehaviour)this).SendRPCInternal("System.Void GameResultManager::RpcRegisterResult(DewGameResult,System.Boolean)", 1458202908, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public void StartTrackingPlayer(DewPlayer p)
	{
		Hero hero = p.hero;
		if ((UnityEngine.Object)(object)hero == null)
		{
			return;
		}
		if (tracked == null)
		{
			StartTrackingLazy();
		}
		int num = tracked.players.FindIndex((DewGameResult.PlayerData t) => t.playerGuid == p.guid);
		DewGameResult.PlayerData data;
		if (num >= 0)
		{
			if (hero.HasData<Ad_RegisteredTracker>())
			{
				Debug.Log("Already tracking player: " + ((UnityEngine.Object)(object)p).name);
				return;
			}
			data = tracked.players[num];
			Debug.Log("Started tracking replaced " + hero.GetActorReadableName());
		}
		else
		{
			data = new DewGameResult.PlayerData();
			tracked.players.Add(data);
			data.playerGuid = p.guid;
			data.platform = p.Platform;
			data.platformID = p.PlatformID;
			Debug.Log("Started tracking " + hero.GetActorReadableName());
		}
		data.playerProfileName = p.playerNameRaw;
		data.heroType = ((object)hero).GetType().Name;
		data.level = hero.level;
		hero.AddData(new Ad_RegisteredTracker());
		p.ClientEvent_OnEarnStardust += (Action<int>)((int amount) =>
		{
			data.totalStardustIncome += amount;
		});
		p.ClientEvent_OnEarnGold += (Action<int>)((int amount) =>
		{
			data.totalGoldIncome += amount;
		});
		p.ClientEvent_OnDreamDustChanged += (Action<int, int>)((int from, int to) =>
		{
			if (to > from)
			{
				data.totalDreamDustIncome += to - from;
			}
		});
		hero.ClientHeroEvent_OnLevelChanged += (Action<EventInfoHeroLevelUp>)((EventInfoHeroLevelUp up) =>
		{
			data.level = up.newLevel;
		});
		hero.ClientHeroEvent_OnKillOrAssist += (Action<EventInfoKill>)((EventInfoKill kill) =>
		{
			if (!hero.IsNullOrInactive() && hero.CheckEnemyOrNeutral(kill.victim) && kill.victim is Monster monster)
			{
				data.kills++;
				if (monster is BossMonster)
				{
					data.heroicBossKills++;
				}
				if (monster.type == Monster.MonsterType.MiniBoss)
				{
					data.miniBossKills++;
				}
				if (monster.isHunter)
				{
					data.hunterKills++;
				}
			}
		});
		hero.ClientHeroEvent_OnKnockedOut += (Action<EventInfoKill>)((EventInfoKill _) =>
		{
			data.deaths++;
		});
		hero.ActorEvent_OnDealDamage += (Action<EventInfoDamage>)((EventInfoDamage dmg) =>
		{
			if (!hero.IsNullOrInactive() && hero.GetRelation(dmg.victim) == EntityRelation.Enemy && !dmg.damage.HasAttr(DamageAttribute.DamageShieldOnly) && !dmg.damage.HasAttr(DamageAttribute.NoTracking))
			{
				data.dealtDamageToEnemies += dmg.damage.amount;
				data.maxDealtSingleDamageToEnemy = Mathf.Max(data.maxDealtSingleDamageToEnemy, dmg.damage.amount + dmg.damage.discardedAmount);
			}
		});
		hero.ActorEvent_OnDoHeal += (Action<EventInfoHeal>)((EventInfoHeal heal) =>
		{
			if ((UnityEngine.Object)(object)heal.target == (UnityEngine.Object)(object)hero)
			{
				data.healToSelf += heal.amount;
			}
			else
			{
				data.healToOthers += heal.amount;
			}
		});
		hero.EntityEvent_OnTakeDamage += (Action<EventInfoDamage>)((EventInfoDamage dmg) =>
		{
			if (!dmg.damage.HasAttr(DamageAttribute.DamageShieldOnly) && !dmg.damage.HasAttr(DamageAttribute.NoTracking))
			{
				data.receivedDamage += dmg.damage.amount;
			}
		});
		hero.ClientHeroEvent_OnKnockedOut += (Action<EventInfoKill>)((EventInfoKill kill) =>
		{
			data.causeOfDeathActor = (((UnityEngine.Object)(object)kill.actor != null) ? ((object)kill.actor).GetType().Name : "");
			data.causeOfDeathEntity = (((UnityEngine.Object)(object)kill.actor != null && (UnityEngine.Object)(object)kill.actor.firstEntity != null) ? ((object)kill.actor.firstEntity).GetType().Name : "");
		});
		((MonoBehaviour)(object)hero).StartCoroutine(CombatTimeRoutine());
		IEnumerator CombatTimeRoutine()
		{
			WaitForSeconds wait = new WaitForSeconds(1f);
			while (tracked != null && !((UnityEngine.Object)(object)this == null) && !hero.IsNullOrInactive())
			{
				if (!NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition && !ManagerBase<CameraManager>.instance.isPlayingCutscene && hero.isInCombat)
				{
					data.combatTime++;
				}
				yield return wait;
			}
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcRegisterResult__DewGameResult__Boolean(DewGameResult result, bool didGameEnd)
	{
		Debug.Log("Received game result from server. didGameEnd: " + didGameEnd);
		current = result;
		current.startTimestamp = _startTimestamp;
		current.runId = NetworkedManagerBase<GameManager>.instance.runId;
		foreach (DewGameResult.PlayerData player in result.players)
		{
			DewGameResult.PlayerData p = player;
			p.isLocalPlayer = _localPlayerGuid == p.playerGuid;
			for (int i = 0; i < p.skills.Count; i++)
			{
				DewGameResult.SkillData value = p.skills[i];
				value.capturedTooltipFields.Clear();
				if (NetworkClient.spawned.TryGetValue(value.netId, out var value2) && ((Component)(object)value2).TryGetComponent(out SkillTrigger component))
				{
					value.CaptureTooltipFields(component);
				}
				p.skills[i] = value;
			}
			for (int j = 0; j < p.gems.Count; j++)
			{
				DewGameResult.GemData value3 = p.gems[j];
				value3.capturedTooltipFields.Clear();
				if (NetworkClient.spawned.TryGetValue(value3.netId, out var value4) && ((Component)(object)value4).TryGetComponent(out Gem component2))
				{
					value3.CaptureTooltipFields(component2);
				}
				p.gems[j] = value3;
			}
			p.capturedStarTooltipFields.Clear();
			Process(StarType.Destruction);
			Process(StarType.Imagination);
			Process(StarType.Life);
			Process(StarType.Flexible);
			void Process(StarType type)
			{
				if (p.loadout == null)
				{
					return;
				}
				foreach (LoadoutStarItem star in p.loadout.GetStarList(type))
				{
					if (!string.IsNullOrEmpty(star.name) && star.level > 0)
					{
						bool flag = false;
						if (type != StarType.Flexible)
						{
							StarEffect byShortTypeName = DewResources.GetByShortTypeName<StarEffect>(star.name, default(ResourceLoadSettings));
							flag = byShortTypeName.type != StarType.Flexible && byShortTypeName.type != type;
						}
						DewLocalization.CaptureDescriptionExpressions(DewLocalization.GetStarDescription(star.name), p.capturedStarTooltipFields, new DewLocalization.DescriptionSettings
						{
							currentLevel = star.level,
							starStrength = (flag ? 0.5f : 1f)
						});
					}
				}
			}
		}
		if (!didGameEnd)
		{
			DewSave.profileMain.lastUnrewardedGameResult = result;
			DewSave.SaveProfileMain();
			return;
		}
		if (result.result == DewGameResult.ResultType.PureWhiteDream || result.result == DewGameResult.ResultType.UnknownFate || result.result == DewGameResult.ResultType.StarlessPath)
		{
			InGameUIManager.instance.SetState("Credits");
		}
		else
		{
			InGameUIManager.instance.SetState("Result");
		}
		DewSave.ConsumeGameResult(result, ref AchievementManager.lastGamePlayReward);
		DewSave.profileMain.lastUnrewardedGameResult = null;
		try
		{
			ClientEvent_OnGameConcluded?.Invoke(result);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		DewSave.SaveProfileMain();
	}

	protected static void InvokeUserCode_RpcRegisterResult__DewGameResult__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRegisterResult called on server.");
		}
		else
		{
			((GameResultManager)(object)obj).UserCode_RpcRegisterResult__DewGameResult__Boolean(GeneratedNetworkCode._Read_DewGameResult(reader), NetworkReaderExtensions.ReadBool(reader));
		}
	}

	static GameResultManager()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(GameResultManager), "System.Void GameResultManager::RpcRegisterResult(DewGameResult,System.Boolean)", (RemoteCallDelegate)InvokeUserCode_RpcRegisterResult__DewGameResult__Boolean);
	}
}
