using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Shrine_Destiny : EditSkillShrine, ICustomInteractable
{
	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncDictionary<string, string> currentItems = new SyncDictionary<string, string>();

	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncDictionary<string, int> rerollCount = new SyncDictionary<string, int>();

	public GameObject fxReroll;

	public GameObject fxHasItem;

	public GameObject fxHasNoItem;

	public GameObject fxDismantleTap;

	public GameObject fxDismantleBreak;

	public GameObject rarityObjTemplate;

	public Transform rarityObjParent;

	public Image gemImage;

	public float rerollQualityCostMultiplier = 0.35f;

	public float costAmpPerUse = 0.25f;

	private GameObject _currentRarityObj;

	public string nameRawText => DewLocalization.GetUIValue("Shrine_Destiny_Name");

	public string interactActionRawText
	{
		get
		{
			string value = default;
			if (((SyncIDictionary<string, string>)(object)currentItems).TryGetValue(DewPlayer.local.guid, ref value) && !string.IsNullOrEmpty(value))
			{
				return DewLocalization.GetUIValue("InGame_Interact_ShrineActivate");
			}
			return "<color=#999>" + DewLocalization.GetUIValue("InGame_Interact_ShrineActivate") + "</color>";
		}
	}

	public bool canAltInteract => true;

	public string interactAltActionRawText => DewLocalization.GetUIValue("Shrine_Destiny_ActionSacrifice");

	protected override void Awake()
	{
		base.Awake();
		((SyncIDictionary<string, string>)(object)currentItems).Callback += OnCurrentItemsChanged;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		OnCurrentItemsChanged((Operation<string, string>)3, DewPlayer.local.guid, CollectionExtensions.GetValueOrDefault<string, string>((IReadOnlyDictionary<string, string>)currentItems, DewPlayer.local.guid));
	}

	private void OnCurrentItemsChanged(Operation<string, string> op, string key, string item)
	{
		if (key != DewPlayer.local.guid)
		{
			return;
		}
		if (string.IsNullOrEmpty(item) || !((SyncIDictionary<string, string>)(object)currentItems).ContainsKey(key))
		{
			FxPlay(fxHasNoItem);
			FxStop(fxHasItem);
			return;
		}
		FxPlayNew(fxReroll);
		Gem byShortTypeName = DewResources.GetByShortTypeName<Gem>(item, ResourceLoadSettings.Light);
		gemImage.sprite = byShortTypeName.icon;
		if (_currentRarityObj != null)
		{
			Object.Destroy(_currentRarityObj);
			_currentRarityObj = null;
		}
		_currentRarityObj = Object.Instantiate(rarityObjTemplate, rarityObjParent);
		DewEffect.TintRecursively(_currentRarityObj, Dew.GetRarityColor(byShortTypeName.rarity));
		_currentRarityObj.SetActive(value: true);
		_currentRarityObj.transform.SetAsFirstSibling();
		FxPlay(fxHasItem);
		FxStop(fxHasNoItem);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		FxStop(fxHasItem);
		if (_currentRarityObj != null)
		{
			_currentRarityObj = null;
			Object.Destroy(_currentRarityObj);
		}
	}

	private void RerollPlayerChoice(DewPlayer player, Rarity minRarity)
	{
		string valueOrDefault = CollectionExtensions.GetValueOrDefault<string, string>((IReadOnlyDictionary<string, string>)currentItems, player.guid);
		Rarity rarity = NetworkedManagerBase<LootManager>.instance.SelectGemRarity();
		if ((int)rarity < (int)minRarity)
		{
			rarity = minRarity;
		}
		for (int i = 0; i < 10; i++)
		{
			NetworkedManagerBase<LootManager>.instance.SelectGemAndQuality(rarity, out var gem, out var _);
			if (!(((object)gem).GetType().Name == valueOrDefault))
			{
				((SyncIDictionary<string, string>)(object)currentItems)[player.guid] = ((object)gem).GetType().Name;
				break;
			}
		}
	}

	public override string GetEditSkillIndicatorRawText()
	{
		return DewLocalization.GetUIValue("Shrine_Destiny_EditSkill");
	}

	public int GetRerollCost(DewPlayer player)
	{
		int currentZoneIndex = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex;
		return Mathf.Max(Mathf.RoundToInt(Mathf.Max((NetworkedManagerBase<LootManager>.instance.gemQualityMinByZoneIndex.rare.Evaluate(currentZoneIndex) + NetworkedManagerBase<LootManager>.instance.gemQualityMaxByZoneIndex.rare.Evaluate(currentZoneIndex)) * 0.5f * rerollQualityCostMultiplier, 20f) * (1f + costAmpPerUse * (float)CollectionExtensions.GetValueOrDefault<string, int>((IReadOnlyDictionary<string, int>)rerollCount, player.guid)) / 10f) * 10, 20);
	}

	public override EditSkillTargetInfo GetTargetInfo(DewPlayer player, GemLocation loc, Gem target)
	{
		if ((Object)(object)target == null)
		{
			return default;
		}
		int rerollCost = GetRerollCost(player);
		if (target.quality - rerollCost < 10)
		{
			return new EditSkillTargetInfo
			{
				actionTypeRawText = DewLocalization.GetUIValue("Shrine_Destiny_ActionSacrifice"),
				tooltipRawText = "<color=#fc7e7e>" + DewLocalization.GetUIValue("Shrine_Destiny_EssenceWillBeDestroyed") + "</color>"
			};
		}
		return new EditSkillTargetInfo
		{
			nextLevel = target.quality - rerollCost,
			actionTypeRawText = DewLocalization.GetUIValue("Shrine_Destiny_ActionSacrifice")
		};
	}

	protected override bool OnActivateEditSkill(DewPlayer player, GemLocation loc, Gem target)
	{
		if (target.IsNullOrInactive())
		{
			return false;
		}
		int rerollCost = GetRerollCost(player);
		if (target.quality - rerollCost < 10)
		{
			FxPlayNewNetworked(fxDismantleBreak, player.hero);
			target.Destroy();
		}
		else
		{
			FxPlayNewNetworked(fxDismantleTap, player.hero);
			target.quality -= rerollCost;
		}
		((SyncIDictionary<string, int>)(object)rerollCount)[player.guid] = CollectionExtensions.GetValueOrDefault<string, int>((IReadOnlyDictionary<string, int>)rerollCount, player.guid) + 1;
		RerollPlayerChoice(player, target.rarity);
		return false;
	}

	public override void OnInteract(Entity entity, bool alt)
	{
		DewPlayer player;
		Gem prefab;
		if (alt)
		{
			base.OnInteract(entity, alt: false);
		}
		else
		{
			if (!((NetworkBehaviour)this).isServer)
			{
				return;
			}
			player = entity.owner;
			string text = default;
			if (!((Object)(object)player == null) && ((SyncIDictionary<string, string>)(object)currentItems).TryGetValue(player.guid, ref text) && !string.IsNullOrEmpty(text))
			{
				prefab = DewResources.GetByShortTypeName<Gem>(text, default(ResourceLoadSettings));
				if (!((Object)(object)prefab == null))
				{
					((MonoBehaviour)(object)this).StartCoroutine(Routine());
				}
			}
		}
		IEnumerator Routine()
		{
			((SyncIDictionary<string, string>)(object)currentItems).Remove(player.guid);
			Vector3 pos = GetRandomSpawnPosition(entity.position);
			DoPostUseRoutines(entity);
			yield return new WaitForSeconds(0.5f);
			Dew.CreateGem(prefab, pos, NetworkedManagerBase<LootManager>.instance.SelectGemQuality(prefab.rarity), player);
		}
	}

	public override Color GetEditSkillBackdropColor()
	{
		return new Color(82f / 255f, 157f / 255f, 1f);
	}

	public override Color GetEditSkillIndicatorColor()
	{
		return new Color(148f / 255f, 194f / 255f, 1f);
	}

	public Shrine_Destiny()
	{
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)currentItems);
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)rerollCount);
	}

	private void MirrorProcessed()
	{
	}
}
