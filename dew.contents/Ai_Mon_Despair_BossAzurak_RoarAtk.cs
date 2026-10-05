using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Despair_BossAzurak_RoarAtk : AbilityInstance
{
	public int pillarCount;

	public int atkCount;

	public float pillarSpawnDelay;

	public float pillarSpawnRadius;

	public float atkDelay;

	public float atkInterval;

	public float jumpDelay;

	public float postDelay;

	public ScalingValue dmgPerAtk;

	public Knockback knockback;

	public GameObject fxCast;

	public GameObject fxTelegraph;

	public GameObject fxRoar;

	public GameObject fxAtk;

	public GameObject fxPillarHit;

	public GameObject fxHit;

	public DewAnimationClip clip;

	public DewAnimationClip endClip;

	public DewAnimationClip jumpClip;

	private List<ActorRef<Mon_Despair_AzurakRollPillar>> _pillars;

	private Channel _channel;

	private int _basePillarCount;

	protected override void Awake()
	{
		base.Awake();
		_basePillarCount = pillarCount;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		pillarCount = _basePillarCount;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		_pillars = new List<ActorRef<Mon_Despair_AzurakRollPillar>>();
		_channel = info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = float.PositiveInfinity
		});
		CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		if (NetworkedManagerBase<GameManager>.instance.GetSpecialSkillChanceMultiplier() >= 0.5f || DewPlayer.gamePlayers.Count >= 3)
		{
			pillarCount *= 2;
		}
		FxPlayNetworked(fxCast);
		info.caster.Animation.PlayAbilityAnimation(clip);
		HashSet<Entity> spawners = NetworkedManagerBase<ActorManager>.instance.allEntities;
		foreach (Entity s in spawners)
		{
			if (s is Mon_Despair_AzurakMonsterSpawner)
			{
				CreateStatusEffect(s, new CastInfo(info.caster, s), (Se_Mon_Despair_BossAzurak_RoarAtk_SafeZoneSpawner b) =>
				{
					b.DestroyOnDestroy(this);
					b.DestroyOnDeath(info.caster);
					b.DestroyOnDeath(s);
				});
			}
		}
		yield return new SI.WaitForSeconds(pillarSpawnDelay);
		for (int i = 0; i < pillarCount; i++)
		{
			Quaternion quaternion = Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 0f);
			Vector3 vector = SingletonBehaviour<Room_BossArena>.instance.center + quaternion * Vector3.forward * pillarSpawnRadius;
			vector = Dew.GetPositionOnGround(vector);
			for (int num = 0; num < 10; num++)
			{
				foreach (Entity item in spawners)
				{
					if (!(Vector3.Distance(item.GetAIAgentPosition(info.caster), vector) > 4f))
					{
						vector = SingletonBehaviour<Room_BossArena>.instance.center + Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 0f) * Vector3.forward * pillarSpawnRadius;
						vector = Dew.GetPositionOnGround(vector);
						break;
					}
				}
			}
			Mon_Despair_AzurakRollPillar mon_Despair_AzurakRollPillar = Dew.SpawnEntity(vector, quaternion, null, Dew.GetClosestAliveHero(vector).owner, info.caster.level, (Mon_Despair_AzurakRollPillar b) =>
			{
				if (((NetworkBehaviour)this).isServer)
				{
					b.EntityEvent_OnDeath += (Action<EventInfoKill>)((EventInfoKill _) =>
					{
						_pillars.RemoveAll((ActorRef<Mon_Despair_AzurakRollPillar> r) => (UnityEngine.Object)(object)r.Get() == (UnityEngine.Object)(object)b);
					});
				}
			});
			_pillars.Add(mon_Despair_AzurakRollPillar);
			CreateStatusEffect(mon_Despair_AzurakRollPillar, new CastInfo(info.caster, mon_Despair_AzurakRollPillar), (Se_Mon_Despair_BossAzurak_RoarAtk_SafeZoneSpawner b) =>
			{
				b.DestroyOnDestroy(this);
				b.DestroyOnDeath(info.caster);
			});
			yield return new SI.WaitForSeconds(0.1f);
		}
		FxPlayNetworked(fxTelegraph, info.caster);
		yield return new SI.WaitForSeconds(jumpDelay);
		info.caster.Animation.PlayAbilityAnimation(jumpClip);
		yield return new SI.WaitForSeconds(atkDelay - jumpDelay);
		info.caster.Animation.PlayAbilityAnimation(endClip);
		FxStopNetworked(fxCast);
		FxStopNetworked(fxTelegraph);
		FxPlayNetworked(fxRoar, info.caster);
		for (int i = 0; i < atkCount; i++)
		{
			info.caster.Animation.PlayAbilityAnimation(jumpClip);
			FxPlayNewNetworked(fxAtk, info.caster);
			ListReturnHandle<Entity> handle;
			foreach (Entity item2 in DewPhysics.OverlapCircleAllEntities(out handle, info.caster.agentPosition, 50f))
			{
				if (item2.IsNullInactiveDeadOrKnockedOut() || (UnityEngine.Object)(object)item2 == (UnityEngine.Object)(object)info.caster || item2.Status.HasStatusEffect<Se_Mon_Despair_BossAzurak_RoarAtk_SafeZone>())
				{
					continue;
				}
				Vector3 normalized = (item2.agentPosition - info.caster.agentPosition).normalized;
				if (item2 is Mon_Despair_AzurakRollPillar)
				{
					PureDamage(item2.maxHealth / (float)atkCount).SetOriginPosition(info.caster.agentPosition).Dispatch(item2);
					FxPlayNewNetworked(fxPillarHit, item2.agentPosition, Quaternion.LookRotation(-normalized));
				}
				else if (item2 is Mon_Despair_AzurakMonsterSpawner)
				{
					PureDamage(item2.maxHealth / (float)atkCount).SetOriginPosition(info.caster.agentPosition).Dispatch(item2);
					FxPlayNewNetworked(fxPillarHit, item2.agentPosition, Quaternion.LookRotation(-normalized));
				}
				else
				{
					DamageData damageData = CreateDamage(DamageData.SourceType.Default, dmgPerAtk).SetOriginPosition(info.caster.agentPosition).SetDirection(normalized);
					if (item2 is Monster)
					{
						damageData.ApplyAmplification(5f);
					}
					damageData.Dispatch(item2);
					FxPlayNewNetworked(fxHit, item2);
				}
				knockback.ApplyWithDirection(normalized, item2);
			}
			handle.Return();
			yield return new SI.WaitForSeconds(atkInterval);
		}
		FxStopNetworked(fxRoar);
		yield return new SI.WaitForSeconds(postDelay);
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (_pillars != null)
		{
			ActorRef<Mon_Despair_AzurakRollPillar>[] array = _pillars.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				ActorRef<Mon_Despair_AzurakRollPillar> r = array[i];
				if (!r.IsNullInactiveDeadOrKnockedOut())
				{
					r.Get().Kill();
				}
			}
			_pillars.Clear();
		}
		if (_channel != null)
		{
			_channel.Cancel();
			_channel = null;
		}
		FxStopNetworked(fxRoar);
		FxStopNetworked(fxTelegraph);
		FxStopNetworked(fxCast);
	}

	private void MirrorProcessed()
	{
	}
}
