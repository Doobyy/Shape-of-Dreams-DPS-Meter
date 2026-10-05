using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Shrine_Entanglement : EditSkillShrine
{
	public Color textColor;

	public Color backdropColor;

	public float skillLevelConvertRatio = 0.5f;

	public float gemQualityConvertRatio = 0.5f;

	public override Color GetEditSkillIndicatorColor()
	{
		return textColor;
	}

	public override Color GetEditSkillBackdropColor()
	{
		return backdropColor;
	}

	public override EditSkillTargetInfo GetTargetInfo(DewPlayer player, HeroSkillLocation loc, SkillTrigger target)
	{
		return new EditSkillTargetInfo
		{
			nextLevel = CalculateSkillLevel(target),
			actionTypeRawText = DewLocalization.GetUIValue("InGame_Interact_GemCombine"),
			rejectReasonRawText = GetRejectReason(target)
		};
	}

	public override EditSkillTargetInfo GetTargetInfo(DewPlayer player, GemLocation loc, Gem target)
	{
		return new EditSkillTargetInfo
		{
			nextLevel = CalculateGemQuality(target),
			actionTypeRawText = DewLocalization.GetUIValue("InGame_Interact_GemCombine"),
			rejectReasonRawText = GetRejectReason(target)
		};
	}

	private string GetRejectReason(SkillTrigger target)
	{
		if (!target.isLevelUpEnabled)
		{
			return DewLocalization.GetUIValue("InGame_Message_CantUpgradeThisSkill");
		}
		if (CalculateSkillLevel(target) == target.level)
		{
			return DewLocalization.GetUIValue("Shrine_Entanglement_Rejection_NoOtherMemory");
		}
		return null;
	}

	private string GetRejectReason(Gem target)
	{
		if (CalculateGemQuality(target) == target.quality)
		{
			return DewLocalization.GetUIValue("Shrine_Entanglement_Rejection_NoOtherEssence");
		}
		return null;
	}

	public int CalculateSkillLevel(SkillTrigger target)
	{
		float level = target.level;
		Process(target.owner.Skill.Q);
		Process(target.owner.Skill.W);
		Process(target.owner.Skill.E);
		Process(target.owner.Skill.R);
		return Mathf.RoundToInt(level);
		void Process(SkillTrigger st)
		{
			if (!((Object)(object)st == null) && !((Object)(object)st == (Object)(object)target))
			{
				level += Mathf.Max(1f, (float)st.level * skillLevelConvertRatio);
			}
		}
	}

	public int CalculateGemQuality(Gem target)
	{
		float num = target.quality;
		foreach (Gem value in target.owner.Skill.gems.Values)
		{
			if (!((Object)(object)value == (Object)(object)target))
			{
				num += Mathf.Max(10f, (float)value.quality * gemQualityConvertRatio);
			}
		}
		return Mathf.RoundToInt(num / 10f) * 10;
	}

	protected override bool OnActivateEditSkill(DewPlayer player, HeroSkillLocation loc, SkillTrigger target)
	{
		TpcAskConfirm(target.owner.owner, target);
		return false;
	}

	protected override bool OnActivateEditSkill(DewPlayer player, GemLocation loc, Gem target)
	{
		TpcAskConfirm(target.owner.owner, target);
		return false;
	}

	[TargetRpc]
	private void TpcAskConfirm(NetworkConnectionToClient conn, Actor target)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)target);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)conn, "System.Void Shrine_Entanglement::TpcAskConfirm(Mirror.NetworkConnectionToClient,Actor)", -1717111675, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	private void CmdConfirm(Actor actor, NetworkConnectionToClient conn = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)actor);
		((NetworkBehaviour)this).SendCommandInternal("System.Void Shrine_Entanglement::CmdConfirm(Actor,Mirror.NetworkConnectionToClient)", 679345035, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	[TargetRpc]
	private void TpcCloseEditMode(NetworkConnectionToClient conn)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)conn, "System.Void Shrine_Entanglement::TpcCloseEditMode(Mirror.NetworkConnectionToClient)", -1198479174, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TpcAskConfirm__NetworkConnectionToClient__Actor(NetworkConnectionToClient conn, Actor target)
	{
		string rawContent;
		if (target is SkillTrigger skillTrigger)
		{
			rawContent = string.Format(DewLocalization.GetUIValue("Shrine_Entanglement_Confirmation_Memory"), ChatManager.GetColoredSkillName(((object)skillTrigger).GetType().Name, skillTrigger.level));
		}
		else
		{
			if (!(target is Gem gem))
			{
				return;
			}
			rawContent = string.Format(DewLocalization.GetUIValue("Shrine_Entanglement_Confirmation_Essence"), ChatManager.GetColoredGemName(((object)gem).GetType().Name, gem.quality));
		}
		ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
		{
			owner = (Object)(object)this,
			validator = () => isActive && CanInteract(DewPlayer.local.hero),
			buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.Cancel),
			defaultButton = DewMessageSettings.ButtonType.Cancel,
			rawContent = rawContent,
			destructiveConfirm = true,
			onClose = (DewMessageSettings.ButtonType b) =>
			{
				if (b == DewMessageSettings.ButtonType.Yes)
				{
					CmdConfirm(target);
				}
			}
		});
	}

	protected static void InvokeUserCode_TpcAskConfirm__NetworkConnectionToClient__Actor(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcAskConfirm called on server.");
		}
		else
		{
			((Shrine_Entanglement)(object)obj).UserCode_TpcAskConfirm__NetworkConnectionToClient__Actor((NetworkConnectionToClient)(object)NetworkClient.connection, NetworkReaderExtensions.ReadNetworkBehaviour<Actor>(reader));
		}
	}

	protected void UserCode_CmdConfirm__Actor__NetworkConnectionToClient(Actor actor, NetworkConnectionToClient conn)
	{
		DewPlayer player = conn.GetPlayer();
		if (actor is SkillTrigger skillTrigger)
		{
			player.hero.Skill.TryGetSkillLocation(skillTrigger, out var type);
			EditSkillTargetInfo targetInfo = GetTargetInfo(player, type, skillTrigger);
			if (targetInfo.cost.HasValue)
			{
				player.Spend(targetInfo.cost.Value);
			}
			skillTrigger.level = targetInfo.nextLevel.Value;
			Process(skillTrigger.owner.Skill.Q);
			Process(skillTrigger.owner.Skill.W);
			Process(skillTrigger.owner.Skill.E);
			Process(skillTrigger.owner.Skill.R);
			NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemUpgraded(skillTrigger.owner, (NetworkBehaviour)(object)skillTrigger);
		}
		else
		{
			if (!(actor is Gem gem))
			{
				return;
			}
			player.hero.Skill.TryGetGemLocation(gem, out var location);
			EditSkillTargetInfo targetInfo2 = GetTargetInfo(player, location, gem);
			if (targetInfo2.cost.HasValue)
			{
				player.Spend(targetInfo2.cost.Value);
			}
			gem.quality = targetInfo2.nextLevel.Value;
			foreach (Gem value in gem.owner.Skill.gems.Values)
			{
				if ((Object)(object)value != (Object)(object)gem)
				{
					value.Destroy();
				}
			}
			NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemUpgraded(gem.owner, (NetworkBehaviour)(object)gem);
		}
		DoPostUseRoutines(conn.GetHero());
		TpcCloseEditMode(conn);
		void Process(SkillTrigger skill)
		{
			if (!skill.IsNullOrInactive() && (Object)(object)skill != (Object)(object)actor)
			{
				skill.Destroy();
			}
		}
	}

	protected static void InvokeUserCode_CmdConfirm__Actor__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdConfirm called on client.");
		}
		else
		{
			((Shrine_Entanglement)(object)obj).UserCode_CmdConfirm__Actor__NetworkConnectionToClient(NetworkReaderExtensions.ReadNetworkBehaviour<Actor>(reader), senderConnection);
		}
	}

	protected void UserCode_TpcCloseEditMode__NetworkConnectionToClient(NetworkConnectionToClient conn)
	{
		ManagerBase<EditSkillManager>.instance.EndEdit();
	}

	protected static void InvokeUserCode_TpcCloseEditMode__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcCloseEditMode called on server.");
		}
		else
		{
			((Shrine_Entanglement)(object)obj).UserCode_TpcCloseEditMode__NetworkConnectionToClient((NetworkConnectionToClient)(object)NetworkClient.connection);
		}
	}

	static Shrine_Entanglement()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected Obj, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(Shrine_Entanglement), "System.Void Shrine_Entanglement::CmdConfirm(Actor,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdConfirm__Actor__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_Entanglement), "System.Void Shrine_Entanglement::TpcAskConfirm(Mirror.NetworkConnectionToClient,Actor)", (RemoteCallDelegate)InvokeUserCode_TpcAskConfirm__NetworkConnectionToClient__Actor);
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_Entanglement), "System.Void Shrine_Entanglement::TpcCloseEditMode(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_TpcCloseEditMode__NetworkConnectionToClient);
	}
}
