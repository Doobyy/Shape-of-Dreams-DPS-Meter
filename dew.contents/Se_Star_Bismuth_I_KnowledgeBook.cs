using System;
using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;

public class Se_Star_Bismuth_I_KnowledgeBook : StarEffect
{
	public int countMin = 1;

	public int countMax = 4;

	public StarScalingValue statRatio;

	public float nonDuplicateBonusAmp = 0.5f;

	public override Type heroType => typeof(Hero_Bismuth);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ClientHeroEvent_OnKillOrAssist += new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ClientHeroEvent_OnKillOrAssist -= new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
		}
	}

	private void ClientHeroEventOnKillOrAssist(EventInfoKill obj)
	{
		ChaosReward[] pool;
		int count;
		if (!hero.IsNullInactiveDeadOrKnockedOut() && obj.victim.IsAnyBoss())
		{
			Shrine_Chaos byType = DewResources.GetByType<Shrine_Chaos>(default(ResourceLoadSettings));
			pool = byType.poolByRarity.Get(Rarity.Common).Where((ChaosReward item) =>
			{
				ChaosRewardType chaosRewardType = item.type;
				return chaosRewardType == ChaosRewardType.MaxHealth || chaosRewardType == ChaosRewardType.AttackDamage || chaosRewardType == ChaosRewardType.AbilityPower || chaosRewardType == ChaosRewardType.AbilityHaste || chaosRewardType == ChaosRewardType.AttackSpeed || chaosRewardType == ChaosRewardType.Armor;
			}).ToArray();
			count = UnityEngine.Random.Range(countMin, countMax + 1);
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			for (int i = 0; i < count; i++)
			{
				Dew.CreateActor(Dew.GetGoodRewardPosition(obj.victim.agentPosition, 4f), Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 0f), null, (Shrine_Se_Star_Bismuth_I_KnowledgeBook_Book b) =>
				{
					int num = UnityEngine.Random.Range(0, pool.Length);
					b.NetworktargetPlayerGuid = hero.owner.guid;
					b.Networktype = pool[num].type;
					float num2 = GetValue(statRatio);
					if (!((Hero_Bismuth)hero).HasSameTravelerMemory())
					{
						num2 *= 1f + nonDuplicateBonusAmp;
					}
					b.Networkamount = DewMath.RandomRoundToInt(pool[num].quantity * num2);
					if (b.amount < 1)
					{
						b.Networkamount = 1;
					}
				});
				yield return new WaitForSeconds(0.15f);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
