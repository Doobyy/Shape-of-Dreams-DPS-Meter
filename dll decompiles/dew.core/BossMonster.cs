using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class BossMonster : Monster
{
	public bool isHiddenBoss;

	public bool playBossKillFeedback = true;

	[NonSerialized]
	[SyncVar]
	public bool skipBossSoulFlow;

	public const float SpecialAttackRevealDuration = 8f;

	public bool NetworkskipBossSoulFlow
	{
		get
		{
			return skipBossSoulFlow;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref skipBossSoulFlow, 256uL, (Action<bool, bool>)null);
		}
	}

	public virtual Type GetUniqueReward()
	{
		return null;
	}

	public virtual float GetUniqueRewardChance()
	{
		if (!isHiddenBoss)
		{
			return 0.1f;
		}
		return 1f;
	}

	public static void RevealStealthedBeforeSpecialAttack()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			if (!allHero.IsNullInactiveDeadOrKnockedOut())
			{
				allHero.Reveal(allHero, 8f);
			}
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((UnityEngine.Object)(object)NetworkedManagerBase<GameSettingsManager>.instance != null)
		{
			MidJoinWaitType midJoinWaitType = NetworkedManagerBase<GameSettingsManager>.instance.midJoinWaitType;
			if (midJoinWaitType == MidJoinWaitType.None || midJoinWaitType == MidJoinWaitType.BeforeBossFight)
			{
				NetworkedManagerBase<GameSettingsManager>.instance.midJoinWaitType = MidJoinWaitType.FightingBoss;
			}
		}
		CreateStatusEffect<Se_AutoDetectPresence>(this, new CastInfo(this));
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer || !((UnityEngine.Object)(object)NetworkedManagerBase<GameSettingsManager>.instance != null) || NetworkedManagerBase<GameSettingsManager>.instance.midJoinWaitType != MidJoinWaitType.FightingBoss)
		{
			return;
		}
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (!((UnityEngine.Object)(object)allEntity == (UnityEngine.Object)(object)this) && !allEntity.IsNullInactiveDeadOrKnockedOut() && allEntity is BossMonster)
			{
				return;
			}
		}
		NetworkedManagerBase<GameSettingsManager>.instance.midJoinWaitType = MidJoinWaitType.AfterBossFight;
	}

	protected override void OnDeath(EventInfoKill info)
	{
		base.OnDeath(info);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (!NetworkedManagerBase<ActorManager>.instance.allEntities.Any((Entity e) => (UnityEngine.Object)(object)e != (UnityEngine.Object)(object)this && e is BossMonster))
		{
			NetworkedManagerBase<GameManager>.instance.isGameTimePausedByGame = true;
		}
		foreach (Entity item in new List<Entity>(NetworkedManagerBase<ActorManager>.instance.allEntities))
		{
			if (item.isActive && !item.Status.isDead && item is Monster { type: not MonsterType.Boss } monster && !(monster is IDontKillOnBossMonsterDeath))
			{
				item.Kill();
			}
		}
		Vector3 pos = agentPosition;
		if (!skipBossSoulFlow)
		{
			((MonoBehaviour)(object)NetworkedManagerBase<ActorManager>.instance).StartCoroutine(RewardRoutine());
		}
		((MonoBehaviour)(object)NetworkedManagerBase<ActorManager>.instance).StartCoroutine(ReviveRoutine());
		IEnumerator ReviveRoutine()
		{
			yield return new WaitForSeconds(1f);
			if (!NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition && !NetworkedManagerBase<GameManager>.instance.isGameConcluded)
			{
				foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
				{
					if ((UnityEngine.Object)(object)gamePlayer.hero != null && gamePlayer.hero.isKnockedOut && (int)Dew.GetNavMeshPathStatus(pos, gamePlayer.hero.agentPosition) != 0)
					{
						Vector3 vector = pos + UnityEngine.Random.insideUnitSphere.Flattened() * 5f;
						vector = Dew.GetPositionOnGround(vector);
						vector = Dew.GetValidAgentDestination_LinearSweep(pos, vector);
						gamePlayer.hero.Control.Teleport(vector);
					}
				}
				yield return new WaitForSeconds(0.3f);
				List<Entity> list = new List<Entity>();
				foreach (DewPlayer gamePlayer2 in DewPlayer.gamePlayers)
				{
					if ((UnityEngine.Object)(object)gamePlayer2.hero != null && gamePlayer2.hero.isKnockedOut)
					{
						list.Add(gamePlayer2.hero);
					}
				}
				foreach (Entity item2 in list)
				{
					if (NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition || NetworkedManagerBase<GameManager>.instance.isGameConcluded)
					{
						yield break;
					}
					item2.CreateAbilityInstance(pos, null, new CastInfo(item2, item2), (Ai_ReviveHero a) =>
					{
						a.reviveHealthMultiplier = 0.4f;
					});
					yield return new WaitForSeconds(0.25f);
				}
			}
		}
		IEnumerator RewardRoutine()
		{
			ListReturnHandle<Rift> handle;
			foreach (Rift item3 in Dew.FindAllActorsOfType(out handle))
			{
				item3.isLocked = true;
			}
			handle.Return();
			yield return new WaitForSeconds(4f);
			if (!NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition)
			{
				Vector3 goodRewardPosition = Dew.GetGoodRewardPosition(pos, 0f);
				Hero closestAliveHero = Dew.GetClosestAliveHero(goodRewardPosition);
				if (Vector2.Distance(closestAliveHero.agentPosition.ToXY(), goodRewardPosition.ToXY()) > 5f || (int)Dew.GetNavMeshPathStatus(closestAliveHero.agentPosition, goodRewardPosition) != 0)
				{
					goodRewardPosition = Dew.GetGoodRewardPosition(closestAliveHero.agentPosition, 4f);
				}
				goodRewardPosition = Dew.GetValidAgentDestination_Closest((!Rift.instance.IsNullOrInactive()) ? Rift.instance.position : closestAliveHero.agentPosition, goodRewardPosition);
				Dew.CreateActor(goodRewardPosition, Quaternion.identity, null, (Shrine_BossSoul soul) =>
				{
					soul.Network_bossTypeName = ((object)this).GetType().Name;
					Type uniqueReward = GetUniqueReward();
					if (uniqueReward != null)
					{
						soul.SetItemReward(uniqueReward, GetUniqueRewardChance());
					}
				});
				yield return null;
				if (NetworkedManagerBase<GameManager>.instance.IsContinueSaveSupported() && !NetworkedManagerBase<ActorManager>.instance.allEntities.Any((Entity e) => (UnityEngine.Object)(object)e != (UnityEngine.Object)(object)this && e is BossMonster))
				{
					NetworkedManagerBase<GameManager>.instance.SaveContinueDataMidRun();
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, skipBossSoulFlow);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, skipBossSoulFlow);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref skipBossSoulFlow, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref skipBossSoulFlow, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
