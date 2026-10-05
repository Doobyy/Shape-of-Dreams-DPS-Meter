using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class PropEnt_Stone_PitchBlackOre : PropEntity, IPrewarmRoomContributor
{
	public void ContributeMonsterPrewarm(Dictionary<Monster, int> counts)
	{
		Mon_DarkCave_CaveBat byType = DewResources.GetByType<Mon_DarkCave_CaveBat>(default(ResourceLoadSettings));
		if (!((Object)(object)byType == null))
		{
			float num = (((Object)(object)NetworkedManagerBase<GameManager>.instance != null) ? NetworkedManagerBase<GameManager>.instance.GetMultiplayerDifficultyFactor(reduceWhenDead: true) : 0f);
			int num2 = Mathf.CeilToInt((8f + num * 2.5f) * 2f);
			counts.TryGetValue(byType, out var value);
			counts[byType] = value + num2;
		}
	}

	protected override void OnDeath(EventInfoKill info)
	{
		base.OnDeath(info);
		if (((NetworkBehaviour)this).isServer)
		{
			LockDestroy();
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(0.5f);
			foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
			{
				if (!gamePlayer.hero.IsNullInactiveDeadOrKnockedOut())
				{
					for (int i = 0; i < 100; i++)
					{
						Rarity key = NetworkedManagerBase<LootManager>.instance.SelectGemRarity();
						string[] array = NetworkedManagerBase<LootManager>.instance.poolGemsByRarity[key].Intersect(NetworkedManagerBase<LootManager>.instance.poolGemsByTag[DescriptionTags.Dark]).ToArray();
						if (array.Length != 0)
						{
							Gem byShortTypeName = DewResources.GetByShortTypeName<Gem>(array[Random.Range(0, array.Length)], default(ResourceLoadSettings));
							if (!((Object)(object)byShortTypeName == null))
							{
								Vector3 goodRewardPosition = Dew.GetGoodRewardPosition(agentPosition);
								Dew.CreateGem(byShortTypeName, goodRewardPosition, Random.Range(1, 6) * 10, gamePlayer);
								yield return null;
								break;
							}
						}
					}
				}
			}
			int batCount = DewMath.RandomRoundToInt((8f + NetworkedManagerBase<GameManager>.instance.GetMultiplayerDifficultyFactor(reduceWhenDead: true) * 2.5f) * Random.Range(0.3f, 2f));
			for (int j = 0; j < batCount; j++)
			{
				Mon_DarkCave_CaveBat mon_DarkCave_CaveBat = Dew.SpawnEntity<Mon_DarkCave_CaveBat>(Dew.GetGoodRewardPosition(agentPosition, 8f), Quaternion.Euler(0f, Random.Range(0, 360), 0f), null, DewPlayer.creep, NetworkedManagerBase<GameManager>.instance.ambientLevel);
				if (Random.value < 0.5f)
				{
					mon_DarkCave_CaveBat.CreateStatusEffect<Se_MirageSkin_Delusion>(mon_DarkCave_CaveBat, new CastInfo(mon_DarkCave_CaveBat));
				}
				yield return new WaitForSeconds(Random.Range(0.05f, 0.35f));
			}
			UnlockDestroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
