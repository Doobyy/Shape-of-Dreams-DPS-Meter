using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Se_Star_I_DismantleReroll : EveryZoneStarEffect
{
	public StarScalingValue maxUses;

	[SaveVar(SaveVarFlags.Default)]
	private int _remainingUses;

	private List<string> _identityPool;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.dismantleProcessor.Add(Processor);
			numberDisplay = _remainingUses;
			showIcon = _remainingUses > 0;
		}
	}

	private void Processor(ref int data, Hero from, Actor to)
	{
		if (_remainingUses <= 0)
		{
			return;
		}
		if (to is SkillTrigger skillTrigger)
		{
			if (skillTrigger.persistentData.GetDataOrDefault("Se_Star_I_DismantleReroll", "dontTrigger", defaultValue: false) || skillTrigger.isCharacterSkill)
			{
				return;
			}
			List<string> list = null;
			list = ((skillTrigger.rarity == Rarity.Identity) ? GetIdentityPool() : NetworkedManagerBase<LootManager>.instance.poolSkillsByRarity[skillTrigger.rarity].ToList());
			list.Remove(((object)skillTrigger).GetType().Name);
			if (list.Count == 0)
			{
				return;
			}
			data = 0;
			Dew.CreateSkillTrigger(DewResources.GetByShortTypeName<SkillTrigger>(list[Random.Range(0, list.Count)], default(ResourceLoadSettings)), skillTrigger.position, skillTrigger.level, from.owner).persistentData.SetData("Se_Star_I_DismantleReroll", "dontTrigger", value: true);
			_remainingUses--;
			numberDisplay = _remainingUses;
			showIcon = _remainingUses > 0;
		}
		if (to is Gem gem && !gem.persistentData.GetDataOrDefault("Se_Star_I_DismantleReroll", "dontTrigger", defaultValue: false))
		{
			List<string> list2 = NetworkedManagerBase<LootManager>.instance.poolGemsByRarity[gem.rarity].ToList();
			list2.Remove(((object)gem).GetType().Name);
			if (list2.Count != 0)
			{
				data = 0;
				Dew.CreateGem(DewResources.GetByShortTypeName<Gem>(list2[Random.Range(0, list2.Count)], default(ResourceLoadSettings)), gem.position, gem.quality, from.owner).persistentData.SetData("Se_Star_I_DismantleReroll", "dontTrigger", value: true);
				_remainingUses--;
				numberDisplay = _remainingUses;
				showIcon = _remainingUses > 0;
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)hero != null)
		{
			hero.dismantleProcessor.Remove(Processor);
		}
	}

	public override void OnNewZoneReached()
	{
		_remainingUses = GetValueInt(maxUses);
		numberDisplay = _remainingUses;
		showIcon = _remainingUses > 0;
	}

	private List<string> GetIdentityPool()
	{
		if (_identityPool != null)
		{
			return _identityPool;
		}
		HashSet<string> loadoutExclusive = new HashSet<string>();
		foreach (Hero item in DewResources.FindAllByType<Hero>(ResourceLoadSettings.Light))
		{
			HeroSkill component = ((Component)(object)item).GetComponent<HeroSkill>();
			if (!((Object)(object)component == null))
			{
				AssetRef<SkillTrigger>[] loadoutTrait = component.loadoutTrait;
				for (int i = 0; i < loadoutTrait.Length; i++)
				{
					AssetRef<SkillTrigger> assetRef = loadoutTrait[i];
					loadoutExclusive.Add(assetRef.typeName);
				}
			}
		}
		_identityPool = NetworkedManagerBase<LootManager>.instance.poolSkillsByRarity[Rarity.Identity].Where((string typeName) => !loadoutExclusive.Contains(typeName) && Dew.IsSkillIncludedInGame(typeName)).ToList();
		return _identityPool;
	}

	private void MirrorProcessed()
	{
	}
}
