using System;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class RoomMod_LeafPuppies : RoomModifierBase
{
	public float spawnPopulationMultiplier = 1.4f;

	public Vector2 sizeMultiplier;

	public float bonusHealthPercentageBase = -30f;

	public float bonusHealthPercentagePerSize = 150f;

	public override void OnStartServer()
	{
		base.OnStartServer();
		SingletonDewNetworkBehaviour<Room>.instance.monsters.OverrideMonsterType(DewResources.GetByType<Mon_Forest_Hound>(default(ResourceLoadSettings)));
		SingletonDewNetworkBehaviour<Room>.instance.monsters.spawnedPopMultiplier *= spawnPopulationMultiplier;
		SingletonDewNetworkBehaviour<Room>.instance.monsters.onAfterSpawn += new Action<Entity>(OnAfterSpawn);
	}

	private void OnAfterSpawn(Entity obj)
	{
		float num = UnityEngine.Random.Range(sizeMultiplier.x, sizeMultiplier.y);
		RpcApplyTransform(obj, num);
		obj.Control.outerRadius *= num;
		obj.Status.AddStatBonus(new StatBonus
		{
			maxHealthPercentage = bonusHealthPercentageBase + bonusHealthPercentagePerSize * (num - 1f)
		});
	}

	[ClientRpc]
	private void RpcApplyTransform(Entity ent, float scale)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)ent);
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, scale);
		((NetworkBehaviour)this).SendRPCInternal("System.Void RoomMod_LeafPuppies::RpcApplyTransform(Entity,System.Single)", -211970482, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null)
		{
			SingletonDewNetworkBehaviour<Room>.instance.monsters.spawnedPopMultiplier /= spawnPopulationMultiplier;
			SingletonDewNetworkBehaviour<Room>.instance.monsters.onAfterSpawn -= new Action<Entity>(OnAfterSpawn);
		}
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
			((RoomMod_LeafPuppies)(object)obj).UserCode_RpcApplyTransform__Entity__Single(NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader), NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	static RoomMod_LeafPuppies()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(RoomMod_LeafPuppies), "System.Void RoomMod_LeafPuppies::RpcApplyTransform(Entity,System.Single)", (RemoteCallDelegate)InvokeUserCode_RpcApplyTransform__Entity__Single);
	}
}
