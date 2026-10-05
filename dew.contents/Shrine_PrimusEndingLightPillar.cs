using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Shrine_PrimusEndingLightPillar : Shrine
{
	protected override bool OnUse(Entity entity)
	{
		int num = 0;
		int num2 = 0;
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			if (!allHero.isKnockedOut)
			{
				num++;
				if (!(Vector2.Distance(allHero.agentPosition.ToXY(), position.ToXY()) > 7f))
				{
					num2++;
				}
			}
		}
		SingletonDewNetworkBehaviour<Primus_Ending>.instance.StartEndingCutscene();
		return true;
	}

	[ClientRpc]
	private void RpcShowNeedPresenceMessage(int curr, int max)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, curr);
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, max);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Shrine_PrimusEndingLightPillar::RpcShowNeedPresenceMessage(System.Int32,System.Int32)", -907751980, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcShowNeedPresenceMessage__Int32__Int32(int curr, int max)
	{
		InGameUIManager.instance.ShowCenterMessage(CenterMessageType.General, "InGame_Message_NeedAllPlayersPresence", new object[2] { curr, max });
	}

	protected static void InvokeUserCode_RpcShowNeedPresenceMessage__Int32__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowNeedPresenceMessage called on server.");
		}
		else
		{
			((Shrine_PrimusEndingLightPillar)(object)obj).UserCode_RpcShowNeedPresenceMessage__Int32__Int32(NetworkReaderExtensions.ReadInt(reader), NetworkReaderExtensions.ReadInt(reader));
		}
	}

	static Shrine_PrimusEndingLightPillar()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_PrimusEndingLightPillar), "System.Void Shrine_PrimusEndingLightPillar::RpcShowNeedPresenceMessage(System.Int32,System.Int32)", (RemoteCallDelegate)InvokeUserCode_RpcShowNeedPresenceMessage__Int32__Int32);
	}
}
