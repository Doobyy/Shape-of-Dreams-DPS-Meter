using System;
using System.Linq;
using Mirror;
using UnityEngine;

public class LucidDream_FishScales : LucidDream
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnHeroAdd += new Action<Hero>(ClientEventOnHeroAdd);
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			ClientEventOnHeroAdd(allHero);
		}
	}

	private void ClientEventOnHeroAdd(Hero h)
	{
		Dew.CallDelayed(() =>
		{
			if (!h.IsNullOrInactive() && !h.Status.HasStatusEffect<Se_FishScales>())
			{
				CreateStatusEffect<Se_FishScales>(h, new CastInfo(h));
			}
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer || !((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null))
		{
			return;
		}
		Entity[] array = NetworkedManagerBase<ActorManager>.instance.allEntities.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Status.TryGetStatusEffect<Se_FishScales>(out var effect))
			{
				effect.Destroy();
			}
		}
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnHeroAdd -= new Action<Hero>(ClientEventOnHeroAdd);
	}

	private void MirrorProcessed()
	{
	}
}
