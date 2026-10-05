using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class GameMod_MirageSkin : GameModifierBase
{
	[Serializable]
	public struct MirageSkinChancePerPlayer
	{
		public float zone0;

		public float zone1;

		public float zone2;

		public float zone3;

		public float zone4;

		public float zone5;

		public float Get(int index)
		{
			if (index <= 0)
			{
				return zone0;
			}
			return index switch
			{
				1 => zone1, 
				2 => zone2, 
				3 => zone3, 
				4 => zone4, 
				_ => zone5, 
			};
		}
	}

	public static GameMod_MirageSkin softInstance;

	public MirageSkinChancePerPlayer[] baseChancesByPlayerCount;

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public List<AssetRef<MirageSkinEffect>> currentZonePool = new List<AssetRef<MirageSkinEffect>>();

	private Action<Entity> _onAfterSpawn;

	public static GameMod_MirageSkin instance => Dew.Helper_GetInstanceOfActor(ref softInstance);

	protected override void OnCreate()
	{
		base.OnCreate();
		softInstance = this;
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
			if ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null)
			{
				ClientEventOnRoomLoaded(default);
			}
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded += new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
			if (currentZonePool.Count == 0)
			{
				RefreshPool();
			}
		}
	}

	private void ClientEventOnZoneLoaded(EventInfoLoadZone obj)
	{
		if (obj.isTraveling)
		{
			RefreshPool();
		}
	}

	public void RefreshPool()
	{
		currentZonePool.Clear();
		Dictionary<int, List<MirageSkinEffect>> skinsByTier = new Dictionary<int, List<MirageSkinEffect>>();
		foreach (MirageSkinEffect item in DewResources.FindAllByType<MirageSkinEffect>(default(ResourceLoadSettings)))
		{
			if (!skinsByTier.TryGetValue(item.tier, out var value))
			{
				value = new List<MirageSkinEffect>();
				skinsByTier.Add(item.tier, value);
			}
			value.Add(item);
		}
		if (NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex <= 2)
		{
			AddByType<Se_MirageSkin_Delusion>();
		}
		else if (NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex <= 5)
		{
			AddByType<Se_MirageSkin_Delusion>();
			AddRandomByTier(0);
		}
		else if (NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex <= 8)
		{
			AddRandomByTier(0);
			AddRandomByTier(1);
		}
		else
		{
			AddRandom();
			AddRandom();
		}
		void AddByType<T>() where T : MirageSkinEffect
		{
			T byType = DewResources.GetByType<T>(default(ResourceLoadSettings));
			currentZonePool.Add(byType);
			if (skinsByTier.TryGetValue(byType.tier, out var value2))
			{
				value2.Remove(byType);
			}
		}
		void AddRandom()
		{
			AddRandomByTier(Dew.SelectRandomWeightedInList(skinsByTier.ToList(), (KeyValuePair<int, List<MirageSkinEffect>> pair) => pair.Value.Count, null).Key);
		}
		void AddRandomByTier(int tier)
		{
			if (skinsByTier.TryGetValue(tier, out var value2) && value2.Count > 0)
			{
				int index = UnityEngine.Random.Range(0, value2.Count);
				MirageSkinEffect mirageSkinEffect = value2[index];
				currentZonePool.Add(mirageSkinEffect);
				value2.RemoveAt(index);
			}
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		SingletonDewNetworkBehaviour<Room>.instance.monsters.onAfterSpawn += new Action<Entity>(OnAfterSpawn);
	}

	private void OnAfterSpawn(Entity obj)
	{
		if (obj is Monster monster && !obj.IsAnyBoss() && monster.type != Monster.MonsterType.Lesser && !monster.Status.HasStatusEffect<MirageSkinEffect>() && currentZonePool.Count != 0)
		{
			DewRandom roomRandom = SingletonDewNetworkBehaviour<Room>.instance.GetRoomRandom(1500);
			float currentBaseMirageChance = GetCurrentBaseMirageChance();
			currentBaseMirageChance += SingletonDewNetworkBehaviour<Room>.instance.monsters.addedMirageChance;
			if (roomRandom.Value() < currentBaseMirageChance)
			{
				obj.CreateStatusEffect(currentZonePool[UnityEngine.Random.Range(0, currentZonePool.Count)].type, obj, new CastInfo(obj));
			}
		}
	}

	public float GetCurrentBaseMirageChance()
	{
		return baseChancesByPlayerCount.GetClamped(Dew.GetAliveHeroCount() - 1).Get(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if ((UnityEngine.Object)(object)softInstance == (UnityEngine.Object)(object)this)
		{
			softInstance = null;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
			{
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded -= new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
			}
			if ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null)
			{
				SingletonDewNetworkBehaviour<Room>.instance.monsters.onAfterSpawn -= new Action<Entity>(OnAfterSpawn);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
