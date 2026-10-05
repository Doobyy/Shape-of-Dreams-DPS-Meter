using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Shrine_BossSoul : EditSkillShrine, IShrineCustomName
{
	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	internal string _bossTypeName;

	[SaveVar(SaveVarFlags.Default)]
	private string _droppedItemTypeName;

	[SaveVar(SaveVarFlags.Default)]
	private float _droppedItemChance;

	public GameObject[] tintedObjects;

	public Color tint;

	public float upgradeStartHeight;

	public float rewardDelay;

	public GameObject fxExplode;

	[SaveVar(SaveVarFlags.Default)]
	private readonly SyncList<string> _usedPlayerIds = new SyncList<string>();

	private bool _startedExplodeRoutine;

	public string Network_bossTypeName
	{
		get
		{
			return _bossTypeName;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref _bossTypeName, 256uL, (Action<string, string>)null);
		}
	}

	protected override void OnCreate()
	{
		GameObject[] array = tintedObjects;
		for (int i = 0; i < array.Length; i++)
		{
			DewEffect.TintRecursively(array[i], tint);
		}
		base.OnCreate();
		FxStop(availableEffect);
		FxPlay(availableEffect);
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = position;
	}

	protected override bool OnUse(Entity entity)
	{
		if (entity is Hero hero && hero.Skill.gems.Count == 0 && hero.Skill.Q.IsNullOrInactive() && hero.Skill.W.IsNullOrInactive() && hero.Skill.E.IsNullOrInactive() && hero.Skill.R.IsNullOrInactive() && hero.Skill.Identity.IsNullOrInactive())
		{
			CreateAbilityInstance(position + Vector3.up * upgradeStartHeight, null, new CastInfo(null, entity), (Ai_RandomGemUpgrade ai) =>
			{
				ai.upgradeTarget = entity;
			});
			_usedPlayerIds.Add(entity.owner.guid);
			CheckIfAllAlivePlayersUpgraded();
			return false;
		}
		return base.OnUse(entity);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			CheckIfAllAlivePlayersUpgraded();
		}
	}

	[ClientRpc]
	private void RpcClearObjective()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Shrine_BossSoul::RpcClearObjective()", 2094961658, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public override bool CanInteract(Entity entity)
	{
		if (base.CanInteract(entity) && (UnityEngine.Object)(object)entity.owner != null && !_usedPlayerIds.Contains(entity.owner.guid))
		{
			return !NetworkedManagerBase<ActorManager>.instance.allEntities.Any((Entity e) => e is BossMonster);
		}
		return false;
	}

	[Server]
	private void CheckIfAllAlivePlayersUpgraded()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Shrine_BossSoul::CheckIfAllAlivePlayersUpgraded()' called when server was not active");
		}
		else
		{
			if (!isActive || _startedExplodeRoutine)
			{
				return;
			}
			foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
			{
				if (!gamePlayer.hero.IsNullInactiveDeadOrKnockedOut() && !_usedPlayerIds.Contains(gamePlayer.guid))
				{
					return;
				}
			}
			Explode();
		}
	}

	[Server]
	private void Explode()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Shrine_BossSoul::Explode()' called when server was not active");
			return;
		}
		_startedExplodeRoutine = true;
		RpcClearObjective();
		Vector3 soulPos = position;
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (rewardDelay > 0.0001f)
			{
				yield return new WaitForSeconds(rewardDelay);
			}
			FxPlayNetworked(fxExplode);
			DewGameplayExperienceSettings ges = NetworkedManagerBase<GameManager>.instance.ges;
			int currentZoneIndex = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex;
			int amount = DewMath.RandomRoundToInt(UnityEngine.Random.Range(ges.bossRewardsGoldMin.Evaluate(currentZoneIndex), ges.bossRewardsGoldMax.Evaluate(currentZoneIndex)));
			int amount2 = DewMath.RandomRoundToInt(UnityEngine.Random.Range(ges.bossRewardsDreamDustMin.Evaluate(currentZoneIndex), ges.bossRewardsDreamDustMax.Evaluate(currentZoneIndex)));
			int stardust = UnityEngine.Random.Range(ges.stardustBossSoulAmount.x, ges.stardustBossSoulAmount.y + 1);
			stardust += NetworkedManagerBase<GameManager>.instance.difficulty.bossKillBonusStardust;
			foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
			{
				if (allActor is LucidDream { type: LucidDreamType.Evil })
				{
					stardust += 3;
				}
			}
			foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
			{
				if (!gamePlayer.hero.IsNullInactiveDeadOrKnockedOut())
				{
					NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: true, isGivenByOtherPlayer: false, amount, soulPos, gamePlayer.hero);
					NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, amount2, soulPos, gamePlayer.hero);
				}
			}
			yield return new WaitForSeconds(0.5f);
			NetworkedManagerBase<PickupManager>.instance.DropStarDust(stardust, soulPos);
			yield return new WaitForSeconds(0.3f);
			if (!string.IsNullOrEmpty(_droppedItemTypeName) && (Dew.IsSkillIncludedInGame(_droppedItemTypeName) || Dew.IsGemIncludedInGame(_droppedItemTypeName)))
			{
				UnityEngine.Object byShortTypeName = DewResources.GetByShortTypeName(_droppedItemTypeName);
				foreach (DewPlayer gamePlayer2 in DewPlayer.gamePlayers)
				{
					if (!gamePlayer2.hero.IsNullInactiveDeadOrKnockedOut() && !(UnityEngine.Random.value > _droppedItemChance))
					{
						Vector3 goodRewardPosition = Dew.GetGoodRewardPosition(soulPos + (gamePlayer2.hero.position - soulPos).normalized * 3f, 1.25f);
						if (byShortTypeName is SkillTrigger skillTrigger)
						{
							NetworkedManagerBase<LootManager>.instance.SelectSkillAndLevel(skillTrigger.rarity, out var _, out var level);
							Dew.CreateSkillTrigger(skillTrigger, goodRewardPosition, level, gamePlayer2);
						}
						else if (byShortTypeName is Gem gem)
						{
							int quality = NetworkedManagerBase<LootManager>.instance.SelectGemQuality(gem.rarity);
							Dew.CreateGem(gem, goodRewardPosition, quality, gamePlayer2);
						}
					}
				}
			}
			yield return new WaitForSeconds(0.4f);
			List<Rift> list = Dew.FindAllActorsOfType(out ListReturnHandle<Rift> handle);
			foreach (Rift item in list)
			{
				item.isLocked = false;
				yield return new WaitForSeconds(0.25f);
			}
			handle.Return();
			Destroy();
		}
	}

	public string GetRawName()
	{
		return string.Format(DewLocalization.GetUIValue("Shrine_BossSoul_Name"), DewLocalization.GetUIValue(_bossTypeName + "_Name"));
	}

	public void SetItemReward<T>(float chance) where T : Gem
	{
		SetItemReward(typeof(T), chance);
	}

	public void SetItemReward(Type type, float chance)
	{
		_droppedItemTypeName = type.Name;
		_droppedItemChance = chance;
	}

	public override string GetEditSkillIndicatorRawText()
	{
		return DewLocalization.GetUIValue("InGame_SelectItemToUpgrade");
	}

	public override EditSkillTargetInfo GetTargetInfo(DewPlayer player, HeroSkillLocation loc, SkillTrigger target)
	{
		return new EditSkillTargetInfo
		{
			nextLevel = target.level + DewResources.GetByType<Ai_RandomGemUpgrade>(default(ResourceLoadSettings)).addedLevel,
			actionTypeRawText = DewLocalization.GetUIValue("InGame_Interact_Upgrade"),
			rejectReasonRawText = ((!target.isLevelUpEnabled) ? DewLocalization.GetUIValue("InGame_Message_CantUpgradeThisSkill") : null)
		};
	}

	public override EditSkillTargetInfo GetTargetInfo(DewPlayer player, GemLocation loc, Gem target)
	{
		return new EditSkillTargetInfo
		{
			nextLevel = target.quality + DewResources.GetByType<Ai_RandomGemUpgrade>(default(ResourceLoadSettings)).addedQuality,
			actionTypeRawText = DewLocalization.GetUIValue("InGame_Interact_Upgrade")
		};
	}

	protected override bool OnActivateEditSkill(DewPlayer player, HeroSkillLocation loc, SkillTrigger target)
	{
		if (_usedPlayerIds.Contains(target.owner.owner.guid))
		{
			return false;
		}
		CreateAbilityInstance(position + Vector3.up * upgradeStartHeight, null, new CastInfo(null, target.owner), (Ai_RandomGemUpgrade ai) =>
		{
			ai.upgradeTarget = target;
		});
		_usedPlayerIds.Add(target.owner.owner.guid);
		CheckIfAllAlivePlayersUpgraded();
		return true;
	}

	protected override bool OnActivateEditSkill(DewPlayer player, GemLocation loc, Gem target)
	{
		if (_usedPlayerIds.Contains(target.owner.owner.guid))
		{
			return false;
		}
		CreateAbilityInstance(position + Vector3.up * upgradeStartHeight, null, new CastInfo(null, target.owner), (Ai_RandomGemUpgrade ai) =>
		{
			ai.upgradeTarget = target;
		});
		_usedPlayerIds.Add(target.owner.owner.guid);
		CheckIfAllAlivePlayersUpgraded();
		return true;
	}

	public Shrine_BossSoul()
	{
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)_usedPlayerIds);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcClearObjective()
	{
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = null;
	}

	protected static void InvokeUserCode_RpcClearObjective(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcClearObjective called on server.");
		}
		else
		{
			((Shrine_BossSoul)(object)obj).UserCode_RpcClearObjective();
		}
	}

	static Shrine_BossSoul()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_BossSoul), "System.Void Shrine_BossSoul::RpcClearObjective()", (RemoteCallDelegate)InvokeUserCode_RpcClearObjective);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteString(writer, _bossTypeName);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, _bossTypeName);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref _bossTypeName, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref _bossTypeName, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
		}
	}
}
