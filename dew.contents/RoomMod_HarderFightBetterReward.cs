using Mirror;
using UnityEngine;

public class RoomMod_HarderFightBetterReward : RoomModifierBase
{
	private class Ad_HarderFight
	{
		public StatBonus bonus;

		public float originalOuterRadius;

		public EntityTransformModifier transformMod;
	}

	public float bonusHealthPercentage;

	public float bonusPowerPercentage;

	public float spawnPopMultiplier;

	public float sizeMultiplier;

	public int skillBonusLevel;

	public int essenceBonusQuality;

	public float addedMirageChance;

	private bool _didSetHighReward;

	public override void OnStartServer()
	{
		base.OnStartServer();
		SingletonDewNetworkBehaviour<Room>.instance.monsters.spawnedPopMultiplier *= spawnPopMultiplier;
		if ((Object)(object)GameMod_MirageSkin.instance != null && GameMod_MirageSkin.instance.GetCurrentBaseMirageChance() > 0.001f)
		{
			SingletonDewNetworkBehaviour<Room>.instance.monsters.addedMirageChance += addedMirageChance;
		}
		SingletonDewNetworkBehaviour<Room>.instance.rewards.skillBonusLevel += skillBonusLevel;
		SingletonDewNetworkBehaviour<Room>.instance.rewards.gemBonusQuality += essenceBonusQuality;
		if (!SingletonDewNetworkBehaviour<Room>.instance.rewards.giveHighRarityReward)
		{
			SingletonDewNetworkBehaviour<Room>.instance.rewards.giveHighRarityReward = true;
			_didSetHighReward = true;
		}
		ModifyEntities((Entity e) =>
		{
			if (e is Monster)
			{
				StatBonus bonus = new StatBonus
				{
					maxHealthPercentage = bonusHealthPercentage,
					attackDamagePercentage = bonusPowerPercentage,
					abilityPowerPercentage = bonusPowerPercentage
				};
				e.Status.AddStatBonus(bonus);
				EntityTransformModifier newTransformModifier = e.Visual.GetNewTransformModifier();
				newTransformModifier.scaleMultiplier = Vector3.one * sizeMultiplier;
				e.AddData(new Ad_HarderFight
				{
					bonus = bonus,
					originalOuterRadius = e.Control.outerRadius,
					transformMod = newTransformModifier
				});
				e.Control.outerRadius *= sizeMultiplier;
			}
		}, (Entity e) =>
		{
			if (e.TryGetData<Ad_HarderFight>(out var data))
			{
				e.Status.RemoveStatBonus(data.bonus);
				data.transformMod.Stop();
				e.Control.outerRadius = data.originalOuterRadius;
				e.RemoveData<Ad_HarderFight>();
			}
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((Object)(object)SingletonDewNetworkBehaviour<Room>.instance == null))
		{
			SingletonDewNetworkBehaviour<Room>.instance.monsters.spawnedPopMultiplier /= spawnPopMultiplier;
			SingletonDewNetworkBehaviour<Room>.instance.monsters.addedMirageChance -= addedMirageChance;
			SingletonDewNetworkBehaviour<Room>.instance.rewards.skillBonusLevel -= skillBonusLevel;
			SingletonDewNetworkBehaviour<Room>.instance.rewards.gemBonusQuality -= essenceBonusQuality;
			if (_didSetHighReward)
			{
				SingletonDewNetworkBehaviour<Room>.instance.rewards.giveHighRarityReward = false;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
