using System;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public abstract class EditSkillShrine : Shrine
{
	public static readonly Color GenericGemIndicatorColor;

	public static readonly Color GenericSkillIndicatorColor;

	public GameObject fxEnterEditMode;

	public GameObject fxEnterEditModeLocal;

	public bool canTargetSkill;

	public bool canTargetGem;

	public bool shouldExitEditModeAfterAction;

	public bool canRepeatConfirmHold;

	private readonly SyncDictionary<string, string> _playerIdToCustomData = new SyncDictionary<string, string>();

	public string GetCustomData(DewPlayer player)
	{
		return CollectionExtensions.GetValueOrDefault<string, string>((IReadOnlyDictionary<string, string>)_playerIdToCustomData, player.guid);
	}

	public virtual string GetEditSkillIndicatorRawText()
	{
		return DewLocalization.GetUIValue(((object)this).GetType().Name + "_EditSkill");
	}

	public virtual Color GetEditSkillIndicatorColor()
	{
		return GenericGemIndicatorColor;
	}

	public virtual Color GetEditSkillBackdropColor()
	{
		return Color.clear;
	}

	public virtual EditSkillTargetType GetTargetTypes(DewPlayer player)
	{
		EditSkillTargetType editSkillTargetType = EditSkillTargetType.None;
		if (canTargetSkill)
		{
			editSkillTargetType |= EditSkillTargetType.Skill;
		}
		if (canTargetGem)
		{
			editSkillTargetType |= EditSkillTargetType.Gem;
		}
		return editSkillTargetType;
	}

	public virtual bool ShouldExitEditModeAfterAction()
	{
		return shouldExitEditModeAfterAction;
	}

	public virtual EditSkillTargetInfo GetTargetInfo(DewPlayer player, HeroSkillLocation loc, SkillTrigger target)
	{
		return default;
	}

	public virtual EditSkillTargetInfo GetTargetInfo(DewPlayer player, GemLocation loc, Gem target)
	{
		return default;
	}

	protected virtual bool OnActivateEditSkill(DewPlayer player, HeroSkillLocation loc, SkillTrigger target)
	{
		return false;
	}

	protected virtual bool OnActivateEditSkill(DewPlayer player, GemLocation loc, Gem target)
	{
		return false;
	}

	protected override bool OnUse(Entity entity)
	{
		EnterEditSkill(entity);
		return false;
	}

	[Server]
	public void EnterEditSkill(Entity entity, string customData = null)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EditSkillShrine::EnterEditSkill(Entity,System.String)' called when server was not active");
			return;
		}
		((SyncIDictionary<string, string>)(object)_playerIdToCustomData)[entity.owner.guid] = customData;
		TpcStartEditSkill(entity.owner);
		FxPlayNewNetworked(fxEnterEditMode, entity);
	}

	public void EnterEditSkill(DewPlayer player, string customData = null)
	{
		EnterEditSkill(player.hero, customData);
	}

	[TargetRpc]
	private void TpcStartEditSkill(NetworkConnectionToClient target)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)target, "System.Void EditSkillShrine::TpcStartEditSkill(Mirror.NetworkConnectionToClient)", 113379108, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	public void CmdActivateEditSkillAction(HeroSkillLocation? skillLoc, GemLocation? gemLoc, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkWriter)(object)val).WriteNullableHeroSkillLocation(skillLoc);
		((NetworkWriter)(object)val).WriteNullableGemLocation(gemLoc);
		((NetworkBehaviour)this).SendCommandInternal("System.Void EditSkillShrine::CmdActivateEditSkillAction(System.Nullable`1<HeroSkillLocation>,System.Nullable`1<GemLocation>,Mirror.NetworkConnectionToClient)", -626233092, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	public override Cost? GetCost(Entity activator)
	{
		return null;
	}

	protected EditSkillShrine()
	{
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)_playerIdToCustomData);
	}

	static EditSkillShrine()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected Obj, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected Obj, but got Unknown
		GenericGemIndicatorColor = new Color(157f / 255f, 14f / 15f, 1f);
		GenericSkillIndicatorColor = new Color(1f, 203f / 255f, 1f / 3f);
		RemoteProcedureCalls.RegisterCommand(typeof(EditSkillShrine), "System.Void EditSkillShrine::CmdActivateEditSkillAction(System.Nullable`1<HeroSkillLocation>,System.Nullable`1<GemLocation>,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdActivateEditSkillAction__Nullable_00601__Nullable_00601__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterRpc(typeof(EditSkillShrine), "System.Void EditSkillShrine::TpcStartEditSkill(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_TpcStartEditSkill__NetworkConnectionToClient);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TpcStartEditSkill__NetworkConnectionToClient(NetworkConnectionToClient target)
	{
		FxPlay(fxEnterEditModeLocal, DewPlayer.local.hero);
		ManagerBase<EditSkillManager>.instance.StartEditSkillShrine(this);
	}

	protected static void InvokeUserCode_TpcStartEditSkill__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcStartEditSkill called on server.");
		}
		else
		{
			((EditSkillShrine)(object)obj).UserCode_TpcStartEditSkill__NetworkConnectionToClient((NetworkConnectionToClient)(object)NetworkClient.connection);
		}
	}

	protected void UserCode_CmdActivateEditSkillAction__Nullable_00601__Nullable_00601__NetworkConnectionToClient(HeroSkillLocation? skillLoc, GemLocation? gemLoc, NetworkConnectionToClient sender)
	{
		try
		{
			if (!skillLoc.HasValue && !gemLoc.HasValue)
			{
				return;
			}
			DewPlayer player = sender.GetPlayer();
			if ((UnityEngine.Object)(object)player == null || !CanInteract(player.hero))
			{
				return;
			}
			player.hero.Control.UpdateLastMoveTime();
			if (skillLoc.HasValue)
			{
				SkillTrigger skill = player.hero.Skill.GetSkill(skillLoc.Value);
				if ((skill.IsNullOrInactive() && !GetTargetTypes(player).HasFlag(EditSkillTargetType.SkillEmptySlot)) || (!skill.IsNullOrInactive() && !GetTargetTypes(player).HasFlag(EditSkillTargetType.Skill)))
				{
					return;
				}
				EditSkillTargetInfo targetInfo = GetTargetInfo(player, skillLoc.Value, skill);
				if (targetInfo.rejectReasonRawText != null || player.hero.Ability.IsAbilityEditLocked((int)skillLoc.Value))
				{
					return;
				}
				AffordType affordType = targetInfo.cost?.CanAfford(skill.owner) ?? AffordType.Yes;
				if (affordType != AffordType.Yes)
				{
					player.TpcShowCenterMessage(CenterMessageType.Error, "InGame_Message_CannotAfford" + affordType);
				}
				else if (OnActivateEditSkill(player, skillLoc.Value, skill))
				{
					if (targetInfo.cost.HasValue)
					{
						player.Spend(targetInfo.cost.Value);
					}
					DoPostUseRoutines(player.hero);
				}
				return;
			}
			Gem gem = player.hero.Skill.GetGem(gemLoc.Value);
			if ((gem.IsNullOrInactive() && !GetTargetTypes(player).HasFlag(EditSkillTargetType.GemEmptySlot)) || (!gem.IsNullOrInactive() && !GetTargetTypes(player).HasFlag(EditSkillTargetType.Gem)))
			{
				return;
			}
			EditSkillTargetInfo targetInfo2 = GetTargetInfo(player, gemLoc.Value, gem);
			if (targetInfo2.rejectReasonRawText != null)
			{
				return;
			}
			AffordType affordType2 = targetInfo2.cost?.CanAfford(gem.owner) ?? AffordType.Yes;
			if (affordType2 != AffordType.Yes)
			{
				player.TpcShowCenterMessage(CenterMessageType.Error, "InGame_Message_CannotAfford" + affordType2);
			}
			else if (OnActivateEditSkill(player, gemLoc.Value, gem))
			{
				if (targetInfo2.cost.HasValue)
				{
					player.Spend(targetInfo2.cost.Value);
				}
				DoPostUseRoutines(player.hero);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_CmdActivateEditSkillAction__Nullable_00601__Nullable_00601__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdActivateEditSkillAction called on client.");
		}
		else
		{
			((EditSkillShrine)(object)obj).UserCode_CmdActivateEditSkillAction__Nullable_00601__Nullable_00601__NetworkConnectionToClient(reader.ReadNullableHeroSkillLocation(), reader.ReadNullableGemLocation(), senderConnection);
		}
	}
}
