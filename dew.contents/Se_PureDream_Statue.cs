using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Se_PureDream_Statue : Se_PureDream
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoUnstoppable();
			RpcApplyTransform(victim, Random.Range(1.2f, 1.3f));
		}
	}

	[ClientRpc]
	private void RpcApplyTransform(Entity ent, float scale)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)ent);
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, scale);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Se_PureDream_Statue::RpcApplyTransform(Entity,System.Single)", -640710747, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcApplyTransform__Entity__Single(Entity ent, float scale)
	{
		ent.Visual.GetNewTransformModifier().scaleMultiplier = Vector3.one * scale;
	}

	protected static void InvokeUserCode_RpcApplyTransform__Entity__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcApplyTransform called on server.");
		}
		else
		{
			((Se_PureDream_Statue)(object)obj).UserCode_RpcApplyTransform__Entity__Single(NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader), NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	static Se_PureDream_Statue()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Se_PureDream_Statue), "System.Void Se_PureDream_Statue::RpcApplyTransform(Entity,System.Single)", (RemoteCallDelegate)InvokeUserCode_RpcApplyTransform__Entity__Single);
	}
}
