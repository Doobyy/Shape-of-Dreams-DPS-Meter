using System;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Shrine_MawOfDoom : EditSkillShrine
{
	public Color textColor;

	public Color backdropColor;

	public Formula destroyChanceByUseCount;

	public Formula goldCostByUseCount;

	[NonSerialized]
	public Dictionary<string, float> priceMultipliersByPlayer = new Dictionary<string, float>();

	[NonSerialized]
	public Dictionary<string, Action<Actor>> destroyCustomActionsByPlayer = new Dictionary<string, Action<Actor>>();

	public float GetDestroyChance(DewPlayer player)
	{
		return destroyChanceByUseCount.Evaluate(GetUseCountByPlayer(player));
	}

	public int GetGoldPrice(DewPlayer player)
	{
		float adjustedGoldAmount_Cost_Service = NetworkedManagerBase<GameManager>.instance.GetAdjustedGoldAmount_Cost_Service(goldCostByUseCount.Evaluate(GetUseCountByPlayer(player)));
		float valueOrDefault = CollectionExtensions.GetValueOrDefault<string, float>((IReadOnlyDictionary<string, float>)priceMultipliersByPlayer, player.guid, 1f);
		return Mathf.RoundToInt(adjustedGoldAmount_Cost_Service * valueOrDefault);
	}

	public override string GetEditSkillIndicatorRawText()
	{
		return DewLocalization.GetUIValue("InGame_SelectItemToUpgrade");
	}

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
			nextLevel = target.level + 1,
			cost = Cost.Gold(GetGoldPrice(target.owner.owner)),
			actionTypeRawText = DewLocalization.GetUIValue("InGame_Interact_Upgrade"),
			tooltipRawText = GetDestroyTooltip(target.owner.owner, isMemory: true),
			rejectReasonRawText = ((!target.isLevelUpEnabled) ? DewLocalization.GetUIValue("InGame_Message_CantUpgradeThisSkill") : null)
		};
	}

	public override EditSkillTargetInfo GetTargetInfo(DewPlayer player, GemLocation loc, Gem target)
	{
		return new EditSkillTargetInfo
		{
			nextLevel = target.quality + NetworkedManagerBase<GameManager>.instance.ges.gemAddedQualityOnUpgrade,
			cost = Cost.Gold(GetGoldPrice(target.owner.owner)),
			actionTypeRawText = DewLocalization.GetUIValue("InGame_Interact_Upgrade"),
			tooltipRawText = GetDestroyTooltip(target.owner.owner, isMemory: false)
		};
	}

	private string GetDestroyTooltip(DewPlayer player, bool isMemory)
	{
		Color editSkillIndicatorColor = GetEditSkillIndicatorColor();
		string arg = $"<color={Dew.GetHex(editSkillIndicatorColor.WithV(1f).WithS(0.2f))}>{GetDestroyChance(player):P0}</color>";
		return "<color=" + Dew.GetHex(editSkillIndicatorColor) + ">" + string.Format(DewLocalization.GetUIValue(isMemory ? "Shrine_MawOfDoom_ChanceTooltip_Memory" : "Shrine_MawOfDoom_ChanceTooltip_Essence"), arg) + "</color>";
	}

	protected override bool OnActivateEditSkill(DewPlayer player, HeroSkillLocation loc, SkillTrigger target)
	{
		if (UnityEngine.Random.value < GetDestroyChance(target.owner.owner))
		{
			TpcCloseEditMode(target.owner.owner);
			CreateAbilityInstance(position, null, default, (Ai_Shrine_MawOfDoom_DestroyTarget ai) =>
			{
				ai.Networktarget = target;
				if (destroyCustomActionsByPlayer.TryGetValue(target.owner.owner.guid, out var action))
				{
					ai.customAction += (Action)(() =>
					{
						action?.Invoke(target);
					});
				}
			});
			return true;
		}
		EditSkillTargetInfo targetInfo = GetTargetInfo(player, loc, target);
		target.level = targetInfo.nextLevel.Value;
		NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemUpgraded(target.owner, (NetworkBehaviour)(object)target);
		return true;
	}

	protected override bool OnActivateEditSkill(DewPlayer player, GemLocation loc, Gem target)
	{
		float num = GetDestroyChance(target.owner.owner);
		if ((double)num < 0.2)
		{
			num -= 0.03f;
		}
		if (UnityEngine.Random.value < num)
		{
			TpcCloseEditMode(target.owner.owner);
			CreateAbilityInstance(position, null, default, (Ai_Shrine_MawOfDoom_DestroyTarget ai) =>
			{
				ai.Networktarget = target;
				if (destroyCustomActionsByPlayer.TryGetValue(target.owner.owner.guid, out var action))
				{
					ai.customAction += (Action)(() =>
					{
						action?.Invoke(target);
					});
				}
			});
			return true;
		}
		EditSkillTargetInfo targetInfo = GetTargetInfo(player, loc, target);
		target.quality = targetInfo.nextLevel.Value;
		NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemUpgraded(target.owner, (NetworkBehaviour)(object)target);
		return true;
	}

	[TargetRpc]
	private void TpcCloseEditMode(NetworkConnectionToClient conn)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)conn, "System.Void Shrine_MawOfDoom::TpcCloseEditMode(Mirror.NetworkConnectionToClient)", 1575056455, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
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
			((Shrine_MawOfDoom)(object)obj).UserCode_TpcCloseEditMode__NetworkConnectionToClient((NetworkConnectionToClient)(object)NetworkClient.connection);
		}
	}

	static Shrine_MawOfDoom()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_MawOfDoom), "System.Void Shrine_MawOfDoom::TpcCloseEditMode(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_TpcCloseEditMode__NetworkConnectionToClient);
	}
}
