using System;
using System.Linq;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_I_ColdSkillOnEnterWorld : StarEffect
{
	public StarScalingValue dropColdSkillLevel;

	public override Type heroType => typeof(Hero_Cetus);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded += new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded -= new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
		}
	}

	private void ClientEventOnZoneLoaded(EventInfoLoadZone obj)
	{
		NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
		{
			if (obj.isTraveling && !hero.IsNullInactiveDeadOrKnockedOut())
			{
				string[] coldCandidates = GetColdCandidates(NetworkedManagerBase<LootManager>.instance.SelectSkillRarity());
				if (coldCandidates.Length == 0)
				{
					Rarity[] array = new Rarity[4]
					{
						Rarity.Common,
						Rarity.Rare,
						Rarity.Epic,
						Rarity.Legendary
					};
					for (int i = 0; i < array.Length; i++)
					{
						coldCandidates = GetColdCandidates(array[i]);
						if (coldCandidates.Length != 0)
						{
							break;
						}
					}
				}
				if (coldCandidates.Length != 0)
				{
					SkillTrigger byShortTypeName = DewResources.GetByShortTypeName<SkillTrigger>(coldCandidates[UnityEngine.Random.Range(0, coldCandidates.Length)], default(ResourceLoadSettings));
					Vector3 pivot = hero.agentPosition + (((Component)(object)Rift.instance).transform.position - hero.agentPosition).normalized * 2.5f;
					pivot = Dew.GetGoodRewardPosition(pivot, 1f);
					Dew.CreateSkillTrigger(byShortTypeName, pivot, GetValueInt(dropColdSkillLevel), player);
				}
			}
		});
	}

	private static string[] GetColdCandidates(Rarity rarity)
	{
		LootManager instance = NetworkedManagerBase<LootManager>.instance;
		if (!instance.poolSkillsByRarity.TryGetValue(rarity, out var value))
		{
			return Array.Empty<string>();
		}
		if (!instance.poolSkillsByTag.TryGetValue(DescriptionTags.Cold, out var value2))
		{
			return Array.Empty<string>();
		}
		return value.Intersect(value2).ToArray();
	}

	private void MirrorProcessed()
	{
	}
}
