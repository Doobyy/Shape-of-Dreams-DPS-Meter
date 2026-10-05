using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_I_ColdDiscountFireSurcharge : StarEffect
{
	public StarScalingValue discountRatio;

	public float surchargeRatio = 1f;

	private const string SaveKey = "Se_Star_Cetus_I_ColdDiscountFireSurcharge";

	private const string SaveValue = "isUpdated";

	private readonly List<(PropEnt_Merchant_Jonas jonas, Action<DewPlayer> handler)> _merchandiseSubscriptions = new List<(PropEnt_Merchant_Jonas, Action<DewPlayer>)>();

	public override Type heroType => typeof(Hero_Cetus);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(ClientEventOnActorAdd);
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			ClientEventOnActorAdd(allActor);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((bool)(UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(ClientEventOnActorAdd);
		}
		foreach (var merchandiseSubscription in _merchandiseSubscriptions)
		{
			if ((UnityEngine.Object)(object)merchandiseSubscription.jonas != null)
			{
				merchandiseSubscription.jonas.onMerchandisePopulated -= merchandiseSubscription.handler;
			}
		}
		_merchandiseSubscriptions.Clear();
	}

	private void ClientEventOnActorAdd(Actor obj)
	{
		PropEnt_Merchant_Jonas jonas = obj as PropEnt_Merchant_Jonas;
		if (jonas == null)
		{
			return;
		}
		Action<DewPlayer> action = (DewPlayer p) =>
		{
			if (!this.IsNullOrInactive() && !((UnityEngine.Object)(object)p != (UnityEngine.Object)(object)player))
			{
				jonas.persistentData.SetData("Se_Star_Cetus_I_ColdDiscountFireSurcharge", "isUpdated", player.guid, value: false);
				UpdatePrice(jonas);
			}
		};
		jonas.onMerchandisePopulated += action;
		_merchandiseSubscriptions.Add((jonas, action));
		UpdatePrice(jonas);
	}

	private void UpdatePrice(PropEnt_Merchant_Jonas target)
	{
		target.persistentData.TryGetData<bool>("Se_Star_Cetus_I_ColdDiscountFireSurcharge", "isUpdated", player.guid, out var value);
		MerchandiseData[] source = default;
		if (value || !((SyncIDictionary<string, MerchandiseData[]>)(object)target.merchandises).TryGetValue(player.guid, ref source))
		{
			return;
		}
		MerchandiseData[] array = source.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			MerchandiseData merchandiseData = array[i];
			float num = 1f;
			switch (merchandiseData.type)
			{
			case MerchandiseType.Skill:
			{
				SkillTrigger byShortTypeName2 = DewResources.GetByShortTypeName<SkillTrigger>(merchandiseData.itemName, default(ResourceLoadSettings));
				if (byShortTypeName2.tags.HasFlag(DescriptionTags.Cold))
				{
					num = 1f - GetValue(discountRatio);
				}
				else if (byShortTypeName2.tags.HasFlag(DescriptionTags.Fire))
				{
					num = 1f + surchargeRatio;
				}
				break;
			}
			case MerchandiseType.Gem:
			{
				Gem byShortTypeName = DewResources.GetByShortTypeName<Gem>(merchandiseData.itemName, default(ResourceLoadSettings));
				if (byShortTypeName.tags.HasFlag(DescriptionTags.Cold))
				{
					num = 1f - GetValue(discountRatio);
				}
				else if (byShortTypeName.tags.HasFlag(DescriptionTags.Fire))
				{
					num = 1f + surchargeRatio;
				}
				break;
			}
			}
			array[i].price = (Cost)(merchandiseData.price * num);
		}
		((SyncIDictionary<string, MerchandiseData[]>)(object)target.merchandises)[player.guid] = array;
		target.persistentData.SetData("Se_Star_Cetus_I_ColdDiscountFireSurcharge", "isUpdated", player.guid, value: true);
	}

	private void MirrorProcessed()
	{
	}
}
