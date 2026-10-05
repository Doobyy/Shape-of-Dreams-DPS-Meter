using System;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class PingManager : NetworkedManagerBase<PingManager>
{
	[Serializable]
	public struct PingPrefabBindings
	{
		public PingInstance world;

		public PingUIInstance ui;
	}

	public struct Ping
	{
		public DewPlayer sender;

		public PingType type;

		public NetworkBehaviour target;

		public Vector3 position;

		public int itemIndex;

		public bool IsValid()
		{
			if ((UnityEngine.Object)(object)sender == null)
			{
				return false;
			}
			switch (type)
			{
			case PingType.Move:
				return true;
			case PingType.Entity:
				if (target is Entity entity)
				{
					return entity.isActive;
				}
				return false;
			case PingType.Interactable:
				if (target is IItem item && ((UnityEngine.Object)(object)item.owner != null || (UnityEngine.Object)(object)item.handOwner != null))
				{
					return false;
				}
				if (target is Actor actor2)
				{
					return actor2.isActive;
				}
				if ((UnityEngine.Object)(object)target != null)
				{
					return ((Behaviour)(object)target).isActiveAndEnabled;
				}
				return false;
			case PingType.EquippedItem:
				if (target is Actor actor)
				{
					return actor.isActive;
				}
				return false;
			case PingType.ShopItem:
				if (target is PropEnt_Merchant_Jonas propEnt_Merchant_Jonas)
				{
					return propEnt_Merchant_Jonas.isActive;
				}
				return false;
			case PingType.WorldNode:
				if (itemIndex >= 0 && itemIndex < NetworkedManagerBase<ZoneManager>.instance.nodes.Count)
				{
					return !NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition;
				}
				return false;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public enum PingType : byte
	{
		Move,
		Entity,
		Interactable,
		EquippedItem,
		ShopItem,
		ChoiceItem,
		WorldNode
	}

	public PingPrefabBindings move;

	public PingPrefabBindings enemy;

	public PingPrefabBindings interest;

	public PingPrefabBindings worldNode;

	public Transform pingIndicatorParent;

	public Transform pingIndicatorAlwaysOnTopParent;

	private Dictionary<DewPlayer, GameObject> _previousPings;

	private float _pingStartTime;

	public override void OnStart()
	{
		base.OnStart();
		_previousPings = new Dictionary<DewPlayer, GameObject>();
		_previousPings.Clear();
	}

	[Command(requiresAuthority = false)]
	public void CmdSendPing(Ping ping, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_PingManager_002FPing((NetworkWriter)(object)val, ping);
		((NetworkBehaviour)this).SendCommandInternal("System.Void PingManager::CmdSendPing(PingManager/Ping,Mirror.NetworkConnectionToClient)", -309149973, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	public void BroadcastPing(Ping ping)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_PingManager_002FPing((NetworkWriter)(object)val, ping);
		((NetworkBehaviour)this).SendRPCInternal("System.Void PingManager::BroadcastPing(PingManager/Ping)", 1814865367, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void ShowPingChatMessage(Ping ping)
	{
		string itemType = "";
		int itemLevel = 0;
		Cost cost = default;
		string itemCustomData = null;
		string text;
		switch (ping.type)
		{
		case PingType.Move:
		case PingType.WorldNode:
		{
			string pingUIValue4 = GetPingUIValue((ping.type == PingType.Move) ? "Chat_Ping_Move" : "Chat_Ping_MoveWorldMap");
			string readableTextForCurrentMode = DewInput.GetReadableTextForCurrentMode(DewSave.profileMain.controls.worldMap);
			ChatManager.Message message = new ChatManager.Message
			{
				type = ChatManager.MessageType.Raw,
				content = "<color=#e4edf0>" + string.Format(pingUIValue4, "<color=" + ChatManager.GetPlayerColorHex(ping.sender) + ">" + ping.sender.playerName + "</color>", readableTextForCurrentMode) + "</color>"
			};
			NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(message);
			return;
		}
		case PingType.Entity:
		{
			Entity entity = (Entity)(object)ping.target;
			string format = GetPingUIValue(accentColor: DewPlayer.local.GetTeamRelation(entity) switch
			{
				TeamRelation.Own => "#52def7", 
				TeamRelation.Neutral => "#faea75", 
				TeamRelation.Enemy => "#fc8888", 
				TeamRelation.Ally => "#52def7", 
				_ => throw new ArgumentOutOfRangeException(), 
			}, key: ((UnityEngine.Object)(object)entity == (UnityEngine.Object)(object)ping.sender.hero) ? "Chat_Ping_Entity_Self" : "Chat_Ping_Entity");
			string text2 = DewLocalization.GetUIValue(((object)entity).GetType().Name + "_Name");
			if (entity is Hero)
			{
				text2 = entity.owner.playerName + " (" + text2 + ")";
			}
			text = string.Format(format, text2, entity.Status.isHealthHidden ? "??" : ((object)Mathf.RoundToInt(entity.normalizedHealth * 100f)));
			break;
		}
		case PingType.Interactable:
		{
			if (ping.target is SkillTrigger skillTrigger3)
			{
				string rarityColorHex3 = Dew.GetRarityColorHex(skillTrigger3.rarity);
				string skillName3 = DewLocalization.GetSkillName(skillTrigger3, 0);
				skillName3 = string.Format(DewLocalization.GetSkillLevelTemplate(skillTrigger3.level), skillName3);
				text = "<color=" + rarityColorHex3 + ">" + skillName3 + "</color>";
				itemType = ((object)skillTrigger3).GetType().Name;
				itemLevel = skillTrigger3.level;
				break;
			}
			if (ping.target is Gem gem2)
			{
				string rarityColorHex4 = Dew.GetRarityColorHex(gem2.rarity);
				Gem.QualityType qualityType4 = Gem.GetQualityType(gem2.quality);
				string gemName4 = DewLocalization.GetGemName(gem2);
				gemName4 = string.Format(DewLocalization.GetUIValue("InGame_Tooltip_GemName_" + qualityType4), $"{gemName4} {gem2.quality}%");
				text = "<color=" + rarityColorHex4 + ">" + gemName4 + "</color>";
				itemType = ((object)gem2).GetType().Name;
				itemLevel = gem2.quality;
				break;
			}
			IInteractable interactable = (IInteractable)ping.target;
			bool flag = interactable.CanInteract(ping.sender.hero);
			if (flag && interactable is Rift { isLocked: not false })
			{
				flag = false;
			}
			if (flag && interactable is Shrine { isLocked: not false })
			{
				flag = false;
			}
			string pingUIValue5 = GetPingUIValue(flag ? "Chat_Ping_Interactable_On" : "Chat_Ping_Interactable_Off", "#ffffff");
			string text3 = null;
			try
			{
				if (interactable is IShrineCustomName shrineCustomName)
				{
					text3 = shrineCustomName.GetRawName();
				}
				else
				{
					text3 = ((!(interactable is ICustomInteractable customInteractable)) ? DewLocalization.GetUIValue(interactable.GetType().Name + "_Name") : customInteractable.nameRawText);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			if (string.IsNullOrWhiteSpace(text3))
			{
				text3 = "???";
			}
			text = string.Format(pingUIValue5, text3);
			break;
		}
		case PingType.EquippedItem:
		{
			string rarityColorHex2 = Dew.GetRarityColorHex((ping.target is SkillTrigger skillTrigger) ? skillTrigger.rarity : ((Gem)(object)ping.target).rarity);
			if (ping.target is SkillTrigger { owner: var owner } skillTrigger2)
			{
				string arg3 = DewLocalization.GetSkillName(skillTrigger2, 0);
				skillTrigger2.owner.Skill.TryGetSkillLocation(skillTrigger2, out var type);
				Gem firstGem = skillTrigger2.owner.Skill.GetFirstGem(type);
				if ((UnityEngine.Object)(object)firstGem != null)
				{
					arg3 = string.Format(DewLocalization.GetGemTemplate(DewLocalization.GetGemKey(((object)firstGem).GetType())), arg3);
				}
				arg3 = string.Format(DewLocalization.GetSkillLevelTemplate(skillTrigger2.level), arg3);
				float num = Mathf.Ceil(skillTrigger2.currentConfigCooldownTime);
				string format2;
				if ((UnityEngine.Object)(object)owner.owner != (UnityEngine.Object)(object)ping.sender)
				{
					format2 = GetPingUIValue("Chat_Ping_Skill_Others", rarityColorHex2);
				}
				else if (skillTrigger2.currentConfigCurrentCharge <= 0)
				{
					format2 = ((skillTrigger2.type != SkillType.Ultimate) ? GetPingUIValue("Chat_Ping_Skill_Cooldown", rarityColorHex2) : GetPingUIValue("Chat_Ping_Skill_Cooldown_Percentage", rarityColorHex2));
				}
				else
				{
					format2 = ((skillTrigger2.currentConfig.maxCharges <= 1) ? GetPingUIValue("Chat_Ping_Skill_Ready", rarityColorHex2) : GetPingUIValue("Chat_Ping_Skill_ReadyMultiple", rarityColorHex2));
				}
				string coloredDescribedPlayerName = ChatManager.GetColoredDescribedPlayerName(owner.owner);
				text = string.Format(format2, arg3, num, skillTrigger2.currentConfigCurrentCharge, coloredDescribedPlayerName, Mathf.RoundToInt(100f - skillTrigger2.currentConfigCooldownTime / skillTrigger2.currentConfigMaxCooldownTime * 100f));
				itemType = ((object)skillTrigger2).GetType().Name;
				itemLevel = skillTrigger2.level;
			}
			else
			{
				if (!(ping.target is Gem { owner: var owner2 } gem))
				{
					return;
				}
				Gem.QualityType qualityType3 = Gem.GetQualityType(gem.quality);
				string gemName3 = DewLocalization.GetGemName(gem);
				gemName3 = string.Format(DewLocalization.GetUIValue("InGame_Tooltip_GemName_" + qualityType3), $"{gemName3} {gem.quality}%");
				string pingUIValue3 = GetPingUIValue(((UnityEngine.Object)(object)owner2.owner != (UnityEngine.Object)(object)ping.sender) ? "Chat_Ping_Gem_Others" : "Chat_Ping_Gem", rarityColorHex2);
				string coloredDescribedPlayerName2 = ChatManager.GetColoredDescribedPlayerName(owner2.owner);
				text = string.Format(pingUIValue3, gemName3, coloredDescribedPlayerName2);
				itemType = ((object)gem).GetType().Name;
				itemLevel = gem.quality;
			}
			break;
		}
		case PingType.ShopItem:
		{
			PropEnt_Merchant_Base propEnt_Merchant_Base = (PropEnt_Merchant_Base)(object)ping.target;
			MerchandiseData merchandiseData = ((SyncIDictionary<string, MerchandiseData[]>)(object)propEnt_Merchant_Base.merchandises)[ping.sender.guid][ping.itemIndex];
			itemCustomData = merchandiseData.customData;
			string arg2;
			string accentColor;
			if (merchandiseData.type == MerchandiseType.Skill)
			{
				string skillLevelTemplate2 = DewLocalization.GetSkillLevelTemplate(merchandiseData.level);
				string skillName2 = DewLocalization.GetSkillName(DewLocalization.GetSkillKey(merchandiseData.itemName), 0);
				arg2 = string.Format(skillLevelTemplate2, skillName2);
				accentColor = Dew.GetRarityColorHex(DewResources.GetByShortTypeName<SkillTrigger>(merchandiseData.itemName, default(ResourceLoadSettings)).rarity);
			}
			else if (merchandiseData.type == MerchandiseType.Gem)
			{
				Gem.QualityType qualityType2 = Gem.GetQualityType(merchandiseData.level);
				string gemName2 = DewLocalization.GetGemName(DewLocalization.GetGemKey(merchandiseData.itemName));
				arg2 = string.Format(DewLocalization.GetUIValue("InGame_Tooltip_GemName_" + qualityType2), $"{gemName2} {merchandiseData.level}%");
				accentColor = Dew.GetRarityColorHex(DewResources.GetByShortTypeName<Gem>(merchandiseData.itemName, default(ResourceLoadSettings)).rarity);
			}
			else if (merchandiseData.type == MerchandiseType.Souvenir)
			{
				arg2 = DewLocalization.GetUIValue(merchandiseData.itemName + "_Name");
				accentColor = Dew.GetHex(new Color(1f, 0.6f, 0.85f));
			}
			else
			{
				if (merchandiseData.type != MerchandiseType.Treasure)
				{
					return;
				}
				arg2 = DewLocalization.GetTreasureName(DewLocalization.GetTreasureKey(merchandiseData.itemName));
				accentColor = Dew.GetHex(new Color(0.4f, 1f, 0.4f));
			}
			string pingUIValue2 = GetPingUIValue((merchandiseData.count > 0) ? "Chat_Ping_ShopItem_HasStock" : "Chat_Ping_ShopItem_OutOfStock", accentColor);
			if (propEnt_Merchant_Base is PropEnt_Merchant_Jonas)
			{
				cost.gold = Mathf.RoundToInt((float)merchandiseData.price.gold * ping.sender.buyPriceMultiplier);
			}
			else
			{
				cost = merchandiseData.price;
			}
			text = "<color=#a7bfc4>" + string.Format(pingUIValue2, arg2, merchandiseData.count, cost) + "</color>";
			itemType = merchandiseData.itemName;
			itemLevel = merchandiseData.level;
			break;
		}
		case PingType.ChoiceItem:
		{
			ChoiceShrine choiceShrine = (ChoiceShrine)(object)ping.target;
			ChoiceShrineItem[] array = default;
			if (!((SyncIDictionary<string, ChoiceShrineItem[]>)(object)choiceShrine.choices).TryGetValue(ping.sender.guid, ref array))
			{
				return;
			}
			ChoiceShrineItem choiceShrineItem = array[ping.itemIndex];
			string arg;
			string rarityColorHex;
			if (choiceShrineItem.typeName.StartsWith("St_"))
			{
				string skillLevelTemplate = DewLocalization.GetSkillLevelTemplate(choiceShrineItem.level);
				string skillName = DewLocalization.GetSkillName(DewLocalization.GetSkillKey(choiceShrineItem.typeName), 0);
				arg = string.Format(skillLevelTemplate, skillName);
				rarityColorHex = Dew.GetRarityColorHex(DewResources.GetByShortTypeName<SkillTrigger>(choiceShrineItem.typeName, default(ResourceLoadSettings)).rarity);
			}
			else
			{
				if (!choiceShrineItem.typeName.StartsWith("Gem_"))
				{
					return;
				}
				Gem.QualityType qualityType = Gem.GetQualityType(choiceShrineItem.level);
				string gemName = DewLocalization.GetGemName(DewLocalization.GetGemKey(choiceShrineItem.typeName));
				arg = string.Format(DewLocalization.GetUIValue("InGame_Tooltip_GemName_" + qualityType), $"{gemName} {choiceShrineItem.level}%");
				rarityColorHex = Dew.GetRarityColorHex(DewResources.GetByShortTypeName<Gem>(choiceShrineItem.typeName, default(ResourceLoadSettings)).rarity);
			}
			string pingUIValue = GetPingUIValue("Chat_Ping_ChoiceShrine", rarityColorHex);
			string uIValue = DewLocalization.GetUIValue(((object)choiceShrine).GetType().Name + "_Name");
			text = "<color=#a7bfc4>" + string.Format(pingUIValue, arg, uIValue) + "</color>";
			itemType = choiceShrineItem.typeName;
			itemLevel = choiceShrineItem.level;
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
		ChatManager.Message message2 = new ChatManager.Message
		{
			type = ChatManager.MessageType.Chat,
			content = "<color=#bcccd1>" + text + "</color>",
			args = new string[1] { ((NetworkBehaviour)ping.sender).netId.ToString() },
			itemType = itemType,
			itemLevel = itemLevel,
			itemPrice = cost,
			itemCustomData = itemCustomData
		};
		NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(message2);
	}

	public static string GetPingUIValue(string key, string accentColor = "#63e3ff")
	{
		return DewLocalization.GetUIValue(key).Replace("[", "<color=" + accentColor + ">").Replace("]", "</color>");
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdSendPing__Ping__NetworkConnectionToClient(Ping ping, NetworkConnectionToClient sender)
	{
		if (!NetworkedManagerBase<ChatManager>.instance.IncrementRateAndCheck(sender))
		{
			NetworkedManagerBase<ChatManager>.instance.SendChatLockedNotice(sender);
			return;
		}
		ping.sender = sender.GetPlayer();
		switch (ping.type)
		{
		default:
			return;
		case PingType.Entity:
			if (!(ping.target is Entity { isActive: not false }) || ping.target.netId == 0)
			{
				return;
			}
			break;
		case PingType.Interactable:
			if ((UnityEngine.Object)(object)ping.target == null || !(ping.target is IInteractable) || ping.target.netId == 0)
			{
				return;
			}
			break;
		case PingType.EquippedItem:
			if ((UnityEngine.Object)(object)ping.target == null || (!(ping.target is SkillTrigger) && !(ping.target is Gem)))
			{
				return;
			}
			break;
		case PingType.ShopItem:
		{
			MerchandiseData[] array = default;
			if (!(ping.target is PropEnt_Merchant_Base propEnt_Merchant_Base) || ping.itemIndex < 0 || !((SyncIDictionary<string, MerchandiseData[]>)(object)propEnt_Merchant_Base.merchandises).TryGetValue(ping.sender.guid, ref array) || ping.itemIndex >= array.Length)
			{
				return;
			}
			break;
		}
		case PingType.ChoiceItem:
		{
			ChoiceShrineItem[] array2 = default;
			if (!(ping.target is ChoiceShrine choiceShrine) || !((SyncIDictionary<string, ChoiceShrineItem[]>)(object)choiceShrine.choices).TryGetValue(ping.sender.guid, ref array2) || ping.itemIndex < 0 || ping.itemIndex >= array2.Length)
			{
				return;
			}
			break;
		}
		case PingType.WorldNode:
			if (ping.itemIndex < 0 || ping.itemIndex >= NetworkedManagerBase<ZoneManager>.instance.nodes.Count)
			{
				return;
			}
			break;
		case PingType.Move:
			break;
		}
		if (ping.target is Gem gem && (UnityEngine.Object)(object)gem.tempOwner == (UnityEngine.Object)(object)sender.GetPlayer())
		{
			gem.tempOwner = null;
		}
		if (ping.target is SkillTrigger skillTrigger && (UnityEngine.Object)(object)skillTrigger.tempOwner == (UnityEngine.Object)(object)sender.GetPlayer())
		{
			skillTrigger.tempOwner = null;
		}
		BroadcastPing(ping);
	}

	protected static void InvokeUserCode_CmdSendPing__Ping__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendPing called on client.");
		}
		else
		{
			((PingManager)(object)obj).UserCode_CmdSendPing__Ping__NetworkConnectionToClient(GeneratedNetworkCode._Read_PingManager_002FPing(reader), senderConnection);
		}
	}

	protected void UserCode_BroadcastPing__Ping(Ping ping)
	{
		if (NetworkedManagerBase<ChatManager>.instance.IsPlayerMuted(ping.sender))
		{
			return;
		}
		if (_previousPings.ContainsKey(ping.sender) && _previousPings[ping.sender] != null)
		{
			UnityEngine.Object.Destroy(_previousPings[ping.sender]);
			_previousPings.Remove(ping.sender);
		}
		ShowPingChatMessage(ping);
		PingPrefabBindings pingPrefabBindings;
		switch (ping.type)
		{
		default:
			return;
		case PingType.Move:
			pingPrefabBindings = move;
			break;
		case PingType.Entity:
		{
			Entity target = (Entity)(object)ping.target;
			pingPrefabBindings = ((!DewPlayer.local.CheckEnemyOrNeutral(target)) ? interest : enemy);
			break;
		}
		case PingType.Interactable:
			pingPrefabBindings = interest;
			break;
		case PingType.WorldNode:
			pingPrefabBindings = worldNode;
			break;
		case PingType.EquippedItem:
		case PingType.ShopItem:
		case PingType.ChoiceItem:
			return;
		}
		if (pingPrefabBindings.world != null)
		{
			PingInstance pingInstance = UnityEngine.Object.Instantiate(pingPrefabBindings.world);
			pingInstance._ping = ping;
			_previousPings[ping.sender] = pingInstance.gameObject;
			if (pingPrefabBindings.ui != null)
			{
				PingUIInstance pingUIInstance = UnityEngine.Object.Instantiate(pingPrefabBindings.ui, pingPrefabBindings.ui.useAlwaysOnTopUIParent ? pingIndicatorAlwaysOnTopParent : pingIndicatorParent);
				pingUIInstance._ping = ping;
				pingInstance._uiInstance = pingUIInstance.gameObject;
			}
		}
		else if (pingPrefabBindings.ui != null)
		{
			PingUIInstance pingUIInstance2 = UnityEngine.Object.Instantiate(pingPrefabBindings.ui, pingPrefabBindings.ui.useAlwaysOnTopUIParent ? pingIndicatorAlwaysOnTopParent : pingIndicatorParent);
			pingUIInstance2._ping = ping;
			_previousPings[ping.sender] = pingUIInstance2.gameObject;
			UnityEngine.Object.Destroy(pingUIInstance2.gameObject, 8f);
		}
	}

	protected static void InvokeUserCode_BroadcastPing__Ping(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC BroadcastPing called on server.");
		}
		else
		{
			((PingManager)(object)obj).UserCode_BroadcastPing__Ping(GeneratedNetworkCode._Read_PingManager_002FPing(reader));
		}
	}

	static PingManager()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(PingManager), "System.Void PingManager::CmdSendPing(PingManager/Ping,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdSendPing__Ping__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterRpc(typeof(PingManager), "System.Void PingManager::BroadcastPing(PingManager/Ping)", (RemoteCallDelegate)InvokeUserCode_BroadcastPing__Ping);
	}
}
