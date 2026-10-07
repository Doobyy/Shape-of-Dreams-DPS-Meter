using System;
using Mirror;
using UnityEngine;

public class LucidDream_TheDarkestUrge : LucidDream
{
	private class Ad_MonsterLevelUp
	{
		public StatBonus bonus;
	}

	public GameObject fxLevelUp;

	public float bonusMaxHealthMult = 1.35f;

	public float bonusAdPercentage = 25f;

	public float bonusApPercentage = 25f;

	public float bonusAtkSpdPercentage = 10f;

	public float bonusMovSpdPercentage = 10f;

	public float bonusAbilHasteFlat = 15f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		UpdateRelations();
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd += new Action<Entity>(OnEntityAdd);
		DewPlayer.onGamePlayerAdded += new Action<DewPlayer>(UpdateRelations);
		DewPlayer.onGamePlayerRemoved += new Action<DewPlayer>(UpdateRelations);
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			OnEntityAdd(allEntity);
		}
	}

	private void UpdateRelations()
	{
		if ((UnityEngine.Object)(object)DewPlayer.creep == null)
		{
			return;
		}
		if (!DewPlayer.creep.enemies.Contains(DewPlayer.creep))
		{
			DewPlayer.creep.enemies.Add(DewPlayer.creep);
		}
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			foreach (DewPlayer gamePlayer2 in DewPlayer.gamePlayers)
			{
				if (!((UnityEngine.Object)(object)gamePlayer == (UnityEngine.Object)(object)gamePlayer2) && !gamePlayer.neutrals.Contains(gamePlayer2))
				{
					gamePlayer.neutrals.Add(gamePlayer2);
				}
			}
			gamePlayer.PurgeRelationsList();
		}
	}

	private void UpdateRelations(DewPlayer _)
	{
		UpdateRelations();
	}

	private void OnEntityAdd(Entity obj)
	{
		if (obj is Monster monster)
		{
			monster.ActorEvent_OnKill += new Action<EventInfoKill>(ActorEventOnKill);
		}
	}

	private void ActorEventOnKill(EventInfoKill obj)
	{
		Entity entity = obj.actor.firstEntity;
		if (!((UnityEngine.Object)(object)entity == null) && !((UnityEngine.Object)(object)obj.victim == null) && !entity.IsAnyBoss() && obj.victim is Monster)
		{
			FxPlayNewNetworked(fxLevelUp, entity);
			if (!entity.TryGetData<Ad_MonsterLevelUp>(out var data))
			{
				data = new Ad_MonsterLevelUp
				{
					bonus = entity.Status.AddStatBonus(new StatBonus())
				};
				entity.AddData(data);
			}
			if (obj.victim.TryGetData<Ad_MonsterLevelUp>(out var data2))
			{
				data.bonus.maxHealthFlat += data2.bonus.maxHealthFlat;
				data.bonus.attackDamagePercentage += data2.bonus.attackDamagePercentage;
				data.bonus.abilityPowerPercentage += data2.bonus.abilityPowerPercentage;
				data.bonus.attackSpeedPercentage += data2.bonus.attackSpeedPercentage;
				data.bonus.movementSpeedPercentage += data2.bonus.movementSpeedPercentage;
				data.bonus.abilityHasteFlat += data2.bonus.abilityHasteFlat;
			}
			data.bonus.maxHealthFlat += obj.victim.Status.baseStats.maxHealth * bonusMaxHealthMult;
			data.bonus.attackDamagePercentage += bonusAdPercentage;
			data.bonus.abilityPowerPercentage += bonusApPercentage;
			data.bonus.attackSpeedPercentage += bonusAtkSpdPercentage;
			data.bonus.movementSpeedPercentage += bonusMovSpdPercentage;
			data.bonus.abilityHasteFlat += bonusAbilHasteFlat;
			entity.Status.CalculateStats();
			entity.Heal(entity.maxHealth).Dispatch(entity);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((UnityEngine.Object)(object)DewPlayer.creep != null)
		{
			DewPlayer.creep.enemies.Remove(DewPlayer.creep);
		}
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			foreach (DewPlayer gamePlayer2 in DewPlayer.gamePlayers)
			{
				if (!((UnityEngine.Object)(object)gamePlayer == (UnityEngine.Object)(object)gamePlayer2))
				{
					gamePlayer.neutrals.Remove(gamePlayer2);
				}
			}
		}
		DewPlayer.onGamePlayerAdded -= new Action<DewPlayer>(UpdateRelations);
		DewPlayer.onGamePlayerRemoved -= new Action<DewPlayer>(UpdateRelations);
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd -= new Action<Entity>(OnEntityAdd);
		}
	}

	private void MirrorProcessed()
	{
	}
}
