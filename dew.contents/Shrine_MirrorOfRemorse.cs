using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Shrine_MirrorOfRemorse : Shrine
{
	public Formula addedLevelByZoneIndex;

	public Formula addedQualityByZoneIndex;

	public GameObject fxDelayedActivate;

	public GameObject fxDelayedActivateLocal;

	protected override bool OnUse(Entity entity)
	{
		TpcAskConfirm(entity.owner);
		return false;
	}

	private int GetAddedLevel()
	{
		return Mathf.RoundToInt(addedLevelByZoneIndex.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex));
	}

	private int GetAddedQuality()
	{
		return Mathf.RoundToInt(addedQualityByZoneIndex.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex));
	}

	[TargetRpc]
	private void TpcAskConfirm(NetworkConnectionToClient conn)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)conn, "System.Void Shrine_MirrorOfRemorse::TpcAskConfirm(Mirror.NetworkConnectionToClient)", -585628213, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[TargetRpc]
	private void TpcPlayLocalActivate(NetworkConnectionToClient conn)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)conn, "System.Void Shrine_MirrorOfRemorse::TpcPlayLocalActivate(Mirror.NetworkConnectionToClient)", -1757240926, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	private void CmdConfirm(NetworkConnectionToClient conn = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void Shrine_MirrorOfRemorse::CmdConfirm(Mirror.NetworkConnectionToClient)", 13766213, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TpcAskConfirm__NetworkConnectionToClient(NetworkConnectionToClient conn)
	{
		ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
		{
			owner = (Object)(object)this,
			validator = () => !this.IsNullOrInactive() && CanInteract(DewPlayer.local.hero),
			buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.Cancel),
			defaultButton = DewMessageSettings.ButtonType.Cancel,
			destructiveConfirm = true,
			rawContent = string.Format(DewLocalization.GetUIValue("Shrine_MirrorOfRemorse_Confirm"), GetAddedLevel().ToString("#,##0"), GetAddedQuality().ToString("#,##0") + "%"),
			onClose = (DewMessageSettings.ButtonType b) =>
			{
				if (b == DewMessageSettings.ButtonType.Yes)
				{
					CmdConfirm();
				}
			}
		});
	}

	protected static void InvokeUserCode_TpcAskConfirm__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcAskConfirm called on server.");
		}
		else
		{
			((Shrine_MirrorOfRemorse)(object)obj).UserCode_TpcAskConfirm__NetworkConnectionToClient((NetworkConnectionToClient)(object)NetworkClient.connection);
		}
	}

	protected void UserCode_TpcPlayLocalActivate__NetworkConnectionToClient(NetworkConnectionToClient conn)
	{
		FxPlayNew(fxDelayedActivateLocal);
	}

	protected static void InvokeUserCode_TpcPlayLocalActivate__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcPlayLocalActivate called on server.");
		}
		else
		{
			((Shrine_MirrorOfRemorse)(object)obj).UserCode_TpcPlayLocalActivate__NetworkConnectionToClient((NetworkConnectionToClient)(object)NetworkClient.connection);
		}
	}

	protected void UserCode_CmdConfirm__NetworkConnectionToClient(NetworkConnectionToClient conn)
	{
		DewPlayer player = conn.GetPlayer();
		Hero hero = conn.GetHero();
		if (CanInteract(hero))
		{
			DoPostUseRoutines(hero);
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(0.6f);
			if (!((Object)(object)player == null) && !hero.IsNullOrInactive())
			{
				CreateBasicEffect(hero, new StunEffect(), 1.5f);
				FxPlayNewNetworked(fxDelayedActivate, hero);
				TpcPlayLocalActivate(conn);
				HeroSkillLocation[] array = new HeroSkillLocation[4]
				{
					HeroSkillLocation.Q,
					HeroSkillLocation.W,
					HeroSkillLocation.E,
					HeroSkillLocation.R
				};
				foreach (HeroSkillLocation type in array)
				{
					if (hero.Skill.TryGetSkill(type, out var skill))
					{
						int level = skill.level + GetAddedLevel();
						Rarity key = (((int)skill.rarity >= 2) ? NetworkedManagerBase<LootManager>.instance.SelectSkillRarity(isHigh: true) : NetworkedManagerBase<LootManager>.instance.SelectSkillRarity());
						List<string> list = NetworkedManagerBase<LootManager>.instance.poolSkillsByRarity[key];
						SkillTrigger skill2 = Dew.CreateSkillTrigger(DewResources.GetByShortTypeName<SkillTrigger>(list[Random.Range(0, list.Count)], default(ResourceLoadSettings)), hero.position, level);
						skill.Destroy();
						hero.Skill.EquipSkill(type, skill2);
					}
				}
				KeyValuePair<GemLocation, Gem>[] array2 = hero.Skill.gems.ToArray();
				for (int i = 0; i < array2.Length; i++)
				{
					KeyValuePair<GemLocation, Gem> keyValuePair = array2[i];
					int quality = keyValuePair.Value.quality + GetAddedQuality();
					Rarity key2 = (((int)keyValuePair.Value.rarity >= 2) ? NetworkedManagerBase<LootManager>.instance.SelectGemRarity(isHigh: true) : NetworkedManagerBase<LootManager>.instance.SelectGemRarity());
					Gem gem = Dew.CreateGem(DewResources.GetByShortTypeName<Gem>(Dew.SelectRandomWeightedInList(NetworkedManagerBase<LootManager>.instance.poolGemsByRarity[key2], (string type2) => (!hero.Skill.HasGemOfType(type2)) ? 1f : 0f, null), default(ResourceLoadSettings)), hero.position, quality);
					keyValuePair.Value.Destroy();
					hero.Skill.EquipGem(keyValuePair.Key, gem);
				}
				hero.Status.SetHealth(hero.maxHealth);
			}
		}
	}

	protected static void InvokeUserCode_CmdConfirm__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdConfirm called on client.");
		}
		else
		{
			((Shrine_MirrorOfRemorse)(object)obj).UserCode_CmdConfirm__NetworkConnectionToClient(senderConnection);
		}
	}

	static Shrine_MirrorOfRemorse()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected Obj, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(Shrine_MirrorOfRemorse), "System.Void Shrine_MirrorOfRemorse::CmdConfirm(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdConfirm__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_MirrorOfRemorse), "System.Void Shrine_MirrorOfRemorse::TpcAskConfirm(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_TpcAskConfirm__NetworkConnectionToClient);
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_MirrorOfRemorse), "System.Void Shrine_MirrorOfRemorse::TpcPlayLocalActivate(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_TpcPlayLocalActivate__NetworkConnectionToClient);
	}
}
