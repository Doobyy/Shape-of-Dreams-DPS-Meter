using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Shrine_Concept : Shrine, IRewardActor
{
	public GameObject fxEmpowered;

	[NonSerialized]
	public Gem gemOverride;

	[NonSerialized]
	public int? qualityOverride;

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
					Rarity value = (SingletonDewNetworkBehaviour<Room>.instance.rewards.giveHighRarityReward ? NetworkedManagerBase<LootManager>.instance.SelectGemRarity(isHigh: true) : NetworkedManagerBase<LootManager>.instance.SelectGemRarity());
					Vector3 vector = positionOverride ?? Dew.GetGoodRewardPosition(pivot);
					NetworkedManagerBase<LootManager>.instance.SelectGemAndQuality(value, out var gem, out var quality);
					quality += SingletonDewNetworkBehaviour<Room>.instance.rewards.gemBonusQuality;
					if ((UnityEngine.Object)(object)gemOverride != null)
					{
						gem = gemOverride;
					}
					if (qualityOverride.HasValue)
					{
						quality = qualityOverride.Value;
					}
					Dew.CreateGem(gem, vector, quality, gamePlayer);
					yield return null;
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
