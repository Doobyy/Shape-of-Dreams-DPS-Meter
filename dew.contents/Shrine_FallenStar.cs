using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Shrine_FallenStar : ChoiceShrine, IRewardActor
{
	private enum VisualState
	{
		Locked,
		Available,
		Unavailable
	}

	public int bonusLevelMin = 2;

	public int bonusLevelMax = 3;

	[Header("Visual")]
	public Shrine_FallenStar_Visual visual;

	public GameObject fractureEffect;

	private VisualState? _appliedVisual;

	private List<string> _identityPool;

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			EnsureChoicesPopulated();
		}
		VisualState visualState = ((!isLocked) ? (isAvailable ? VisualState.Available : VisualState.Unavailable) : VisualState.Locked);
		if (_appliedVisual != visualState)
		{
			ApplyVisual(visualState, !_appliedVisual.HasValue);
			_appliedVisual = visualState;
		}
	}

	private void EnsureChoicesPopulated()
	{
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			if (!((Object)(object)gamePlayer == null) && !((Object)(object)gamePlayer.hero == null) && !((SyncIDictionary<string, ChoiceShrineItem[]>)(object)choices).ContainsKey(gamePlayer.guid))
			{
				PopulatePlayerChoices(gamePlayer);
			}
		}
	}

	private void ApplyVisual(VisualState s, bool instant)
	{
		switch (s)
		{
		case VisualState.Locked:
			if (model != null)
			{
				model.SetActive(value: true);
			}
			FxStop(unlockEffect);
			visual?.Snap(assembled: false);
			FxPlay(lockedEffect);
			break;
		case VisualState.Available:
			if (model != null)
			{
				model.SetActive(value: true);
			}
			FxStop(lockedEffect);
			if (instant)
			{
				visual?.Snap(assembled: true);
			}
			FxPlay(unlockEffect);
			break;
		case VisualState.Unavailable:
			FxStop(lockedEffect);
			FxStop(unlockEffect);
			if (instant)
			{
				if (model != null)
				{
					model.SetActive(value: false);
				}
			}
			else
			{
				FxPlay(fractureEffect);
			}
			FxPlay(unavailableEffect);
			break;
		}
	}

	protected override void OnPopulateChoices(DewPlayer player)
	{
		Hero hero = player.hero;
		if ((Object)(object)hero == null)
		{
			return;
		}
		List<string> identityPool = GetIdentityPool();
		if (identityPool.Count == 0)
		{
			return;
		}
		SkillTrigger identity = hero.Skill.Identity;
		int num = (identity.IsNullOrInactive() ? (-1) : identity.level);
		List<string> list = new List<string>(identityPool);
		list.Shuffle();
		List<ChoiceShrineItem> list2 = DewPool.GetList(out ListReturnHandle<ChoiceShrineItem> handle);
		for (int i = 0; i < itemCount && i < list.Count; i++)
		{
			int num2 = NetworkedManagerBase<LootManager>.instance.SelectSkillLevel(Rarity.Identity) + SingletonDewNetworkBehaviour<Room>.instance.rewards.skillBonusLevel;
			if (num >= 0)
			{
				int num3 = Random.Range(bonusLevelMin, bonusLevelMax + 1);
				num2 = Mathf.Max(num2, num + num3);
			}
			list2.Add(new ChoiceShrineItem
			{
				typeName = list[i],
				level = num2
			});
		}
		((SyncIDictionary<string, ChoiceShrineItem[]>)(object)choices)[player.guid] = list2.ToArray();
		handle.Return();
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
