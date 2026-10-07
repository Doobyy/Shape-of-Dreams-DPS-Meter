using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Shrine_Memory : Shrine, IRewardActor
{
	public GameObject fxEmpowered;

	[NonSerialized]
	public SkillTrigger skillOverride;

	[NonSerialized]
	public int? levelOverride;

	[NonSerialized]
	public Vector3? positionOverride;

	public override bool isRegularReward => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return null;
			if (((NetworkBehaviour)this).isServer && SingletonDewNetworkBehaviour<Room>.instance.rewards.giveHighRarityReward && isAvailable)
			{
				FxPlayNetworked(fxEmpowered);
			}
		}
	}

	protected override bool OnUse(Entity entity)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		return true;
		IEnumerator Routine()
		{
			FxStopNetworked(fxEmpowered);
			Vector3 pivot = GetRandomSpawnPosition(entity.position);
			yield return new WaitForSeconds(0.5f);
			foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
			{
				if (!gamePlayer.hero.IsNullInactiveDeadOrKnockedOut())
				{
					Rarity value = (SingletonDewNetworkBehaviour<Room>.instance.rewards.giveHighRarityReward ? NetworkedManagerBase<LootManager>.instance.SelectSkillRarity(isHigh: true) : NetworkedManagerBase<LootManager>.instance.SelectSkillRarity());
					Vector3 vector = positionOverride ?? Dew.GetGoodRewardPosition(pivot);
					NetworkedManagerBase<LootManager>.instance.SelectSkillAndLevel(value, out var skill, out var level);
					level += SingletonDewNetworkBehaviour<Room>.instance.rewards.skillBonusLevel;
					if ((UnityEngine.Object)(object)skillOverride != null)
					{
						skill = skillOverride;
					}
					if (levelOverride.HasValue)
					{
						level = levelOverride.Value;
					}
					Dew.CreateSkillTrigger(skill, vector, level, gamePlayer);
					yield return null;
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
