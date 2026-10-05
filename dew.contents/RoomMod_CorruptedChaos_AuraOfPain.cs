using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class RoomMod_CorruptedChaos_AuraOfPain : RoomModifierBase
{
	public float healthAmp = 0.2f;

	public float damageAmp = 0.3f;

	public float popMultiplier = 1.4f;

	public float damageRatioPerSecond = 0.01f;

	public float damageTickInterval = 0.1f;

	private float _lastDamageTime;

	private RoomSection _section;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ModifyEntities((Entity ent) =>
		{
			ent.CreateStatusEffect<Se_LimboBossDecorator>(ent, default);
			if (ent is Monster)
			{
				ent.Status.AddStatBonus(new StatBonus
				{
					maxHealthPercentage = healthAmp * 100f,
					abilityPowerPercentage = damageAmp * 100f,
					attackDamagePercentage = damageAmp * 100f
				});
				ent.AI.predictionStrengthOverride = () => UnityEngine.Random.value;
			}
		}, (Entity ent) =>
		{
			if (ent.Status.TryGetStatusEffect<Se_LimboBossDecorator>(out var effect))
			{
				effect.Destroy();
			}
		});
		Rift_RoomExit exitPortal = Rift_RoomExit.instance;
		if ((UnityEngine.Object)(object)exitPortal == null)
		{
			return;
		}
		SingletonDewNetworkBehaviour<Room>.instance.monsters.spawnedPopMultiplier *= popMultiplier;
		SingletonDewNetworkBehaviour<Room>.instance.monsters.maxPopulationMultiplier *= popMultiplier;
		_section = Dew.SelectBestWithScore(SingletonDewNetworkBehaviour<Room>.instance.sections, (RoomSection section, int i) => 0f - Vector2.Distance(((Component)(object)exitPortal).transform.position.ToXY(), section.transform.position.ToXY()));
		if (_section == null || _section.monsters.spawnMiniBossInstead)
		{
			return;
		}
		_section.monsters.spawnMiniBossInstead = true;
		_section.monsters.miniBossCount = 2;
		SingletonDewNetworkBehaviour<Room>.instance.monsters.disableMiniBossRewards = true;
		RefValue<int> remainingCount = new RefValue<int>(2);
		SingletonDewNetworkBehaviour<Room>.instance.monsters.onAfterSpawn += (Action<Entity>)((Entity e) =>
		{
			Dew.CallDelayed(() =>
			{
				if (e is Monster { type: Monster.MonsterType.MiniBoss } monster)
				{
					monster.EntityEvent_OnDeath += (Action<EventInfoKill>)((EventInfoKill kill) =>
					{
						remainingCount.value--;
						if (remainingCount.value == 0)
						{
							((MonoBehaviour)(object)this).StartCoroutine(Routine());
						}
					});
				}
			});
			IEnumerator Routine()
			{
				Vector3 pos = e.agentPosition;
				yield return new WaitForSeconds(1.5f);
				Dew.CreateActor<Shrine_CorruptedChaos>(Dew.GetGoodRewardPosition(pos), null);
				foreach (DewPlayer p in DewPlayer.gamePlayers)
				{
					if (!p.hero.IsNullInactiveDeadOrKnockedOut() && !(SingletonDewNetworkBehaviour<Room>.instance.GetRoomRandom((int)p.guid.GetStableHashCode()).Value() > p.doubleChaosChance))
					{
						Dew.CreateActor(Dew.GetGoodRewardPosition(pos), null, null, (Shrine_CorruptedChaos chaos) =>
						{
							chaos.playersOverride = new string[1] { p.guid };
						});
					}
				}
			}
		});
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || SingletonDewNetworkBehaviour<Room>.instance.didClearRoom || Time.time - _lastDamageTime < damageTickInterval)
		{
			return;
		}
		_lastDamageTime = Time.time;
		float num = damageRatioPerSecond * damageTickInterval;
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			if (!allHero.IsNullInactiveDeadOrKnockedOut())
			{
				float b = allHero.currentHealth + allHero.Status.currentShield - 1f;
				float num2 = Mathf.Min((allHero.maxHealth + allHero.Status.currentShield) * num, b);
				if (!(num2 < 0.1f))
				{
					PureDamage(num2, 0f).SetAttr(DamageAttribute.DamageOverTime).SetAttr(DamageAttribute.IgnoreArmor).Dispatch(allHero);
				}
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if (_section != null)
			{
				_section.monsters.spawnMiniBossInstead = false;
				_section = null;
			}
			if ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null)
			{
				SingletonDewNetworkBehaviour<Room>.instance.monsters.spawnedPopMultiplier /= popMultiplier;
				SingletonDewNetworkBehaviour<Room>.instance.monsters.maxPopulationMultiplier /= popMultiplier;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
