using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Shrine_Hatred : Shrine
{
	public float[] hatredStrengthChances;

	public int choiceCount = 3;

	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncList<HatredStrengthType> choices = new SyncList<HatredStrengthType>();

	private AssetRef<CurseStatusEffect>[] _curses;

	public static int GetIndexOfStrength(HatredStrengthType strength)
	{
		return strength switch
		{
			HatredStrengthType.None => 0, 
			HatredStrengthType.Mild => 1, 
			HatredStrengthType.Potent => 2, 
			HatredStrengthType.Powerful => 3, 
			_ => throw new ArgumentOutOfRangeException("strength", strength, null), 
		};
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		_curses = (from c in DewResources.FindAllByType<CurseStatusEffect>(ResourceLoadSettings.Light)
			where Dew.IsCurseIncludedInGame(((object)c).GetType().Name)
			select c).ToArray().ToAssetRefs<CurseStatusEffect>();
		if (choices.Count != 0)
		{
			return;
		}
		choices.Add(HatredStrengthType.None);
		choices.Add(HatredStrengthType.Mild);
		choices.Add(HatredStrengthType.Potent);
		choices.Add(HatredStrengthType.Powerful);
		while (choices.Count > choiceCount)
		{
			HatredStrengthType hatredStrengthType = Dew.SelectRandomWeightedInList((IList<HatredStrengthType>)choices, (HatredStrengthType t) => 1f / hatredStrengthChances[GetIndexOfStrength(t)]);
			choices.Remove(hatredStrengthType);
		}
	}

	protected override bool OnUse(Entity entity)
	{
		TpcOpenWindow(((NetworkBehaviour)entity.owner).connectionToClient);
		return false;
	}

	[TargetRpc]
	private void TpcOpenWindow(NetworkConnectionToClient target)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)target, "System.Void Shrine_Hatred::TpcOpenWindow(Mirror.NetworkConnectionToClient)", -1077927823, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	public void CmdChoose(int index, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, index);
		((NetworkBehaviour)this).SendCommandInternal("System.Void Shrine_Hatred::CmdChoose(System.Int32,Mirror.NetworkConnectionToClient)", 1341025742, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	private void DoCurse(HatredStrengthType strength, Hero hero)
	{
		if (strength == HatredStrengthType.None)
		{
			return;
		}
		CurseStatusEffect asset = Dew.SelectRandomWeightedInList(_curses, (AssetRef<CurseStatusEffect> r) =>
		{
			CurseStatusEffect asset2 = r.asset;
			if (!asset2.availableStrengths.HasFlag(strength))
			{
				return 0f;
			}
			return (!asset2.IsViable(hero)) ? 0f : asset2.chanceWeight;
		}, null).asset;
		if ((UnityEngine.Object)(object)asset == null)
		{
			return;
		}
		hero.CreateStatusEffect(asset, hero, new CastInfo(hero, hero), (CurseStatusEffect curse) =>
		{
			curse.currentStrength = strength;
			curse.skillLevel = (int)(strength - 1);
			if (UnityEngine.Random.value < 0.5f)
			{
				curse.progressType = QuestProgressType.Kills;
				switch (strength)
				{
				case HatredStrengthType.Mild:
					curse.requiredAmount = 28 + NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex * 2;
					break;
				case HatredStrengthType.Potent:
					curse.requiredAmount = 34 + NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex * 3;
					break;
				case HatredStrengthType.Powerful:
					curse.requiredAmount = 50 + NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex * 3;
					break;
				default:
					throw new ArgumentOutOfRangeException("strength", strength, null);
				}
			}
			else
			{
				curse.progressType = QuestProgressType.Travel;
				switch (strength)
				{
				case HatredStrengthType.Mild:
					curse.requiredAmount = 3;
					break;
				case HatredStrengthType.Potent:
					curse.requiredAmount = 4;
					break;
				case HatredStrengthType.Powerful:
					curse.requiredAmount = 4;
					break;
				default:
					throw new ArgumentOutOfRangeException("strength", strength, null);
				}
			}
		});
	}

	public Shrine_Hatred()
	{
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)choices);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TpcOpenWindow__NetworkConnectionToClient(NetworkConnectionToClient target)
	{
		ManagerBase<FloatingWindowManager>.instance.SetTarget((MonoBehaviour)(object)this);
	}

	protected static void InvokeUserCode_TpcOpenWindow__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcOpenWindow called on server.");
		}
		else
		{
			((Shrine_Hatred)(object)obj).UserCode_TpcOpenWindow__NetworkConnectionToClient((NetworkConnectionToClient)(object)NetworkClient.connection);
		}
	}

	protected void UserCode_CmdChoose__Int32__NetworkConnectionToClient(int index, NetworkConnectionToClient sender)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (index >= 0 && index < choices.Count)
			{
				DewPlayer player = sender.GetPlayer();
				if (!((UnityEngine.Object)(object)player == null) && !player.hero.IsNullInactiveDeadOrKnockedOut() && CanInteract(player.hero))
				{
					DoPostUseRoutines(player.hero);
					if (choices[index] != HatredStrengthType.None)
					{
						yield return new WaitForSeconds(0.75f);
						DoCurse(choices[index], player.hero);
					}
					yield return new WaitForSeconds(0.8f);
					Vector3 goodRewardPosition = Dew.GetGoodRewardPosition(position + (player.hero.agentPosition - position).normalized * 2.5f);
					CreateActor(goodRewardPosition, null, (Shrine_Hatred_Reward r) =>
					{
						r.NetworkplayerGuid = player.guid;
						r.Networktype = choices[index];
					});
				}
			}
		}
	}

	protected static void InvokeUserCode_CmdChoose__Int32__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChoose called on client.");
		}
		else
		{
			((Shrine_Hatred)(object)obj).UserCode_CmdChoose__Int32__NetworkConnectionToClient(NetworkReaderExtensions.ReadInt(reader), senderConnection);
		}
	}

	static Shrine_Hatred()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(Shrine_Hatred), "System.Void Shrine_Hatred::CmdChoose(System.Int32,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdChoose__Int32__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_Hatred), "System.Void Shrine_Hatred::TpcOpenWindow(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_TpcOpenWindow__NetworkConnectionToClient);
	}
}
