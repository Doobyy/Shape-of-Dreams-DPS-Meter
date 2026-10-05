using System;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class RoomMod_MeteoricLife : RoomModifierBase
{
	public float spawnPopulationMultiplier = 1.56f;

	public Vector2 sizeMultiplier;

	public float bonusHealthPercentageBase = -30f;

	public float bonusHealthPercentagePerSize = 150f;

	public override void OnStartServer()
	{
		base.OnStartServer();
		SingletonDewNetworkBehaviour<Room>.instance.monsters.OverrideMonsterType(DewResources.GetByType<Mon_LavaLand_SuperheatedWolf>(default(ResourceLoadSettings)));
		SingletonDewNetworkBehaviour<Room>.instance.monsters.spawnedPopMultiplier *= spawnPopulationMultiplier;
		SingletonDewNetworkBehaviour<Room>.instance.monsters.onAfterSpawn += new Action<Entity>(OnAfterSpawn);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance == null))
		{
			SingletonDewNetworkBehaviour<Room>.instance.monsters.spawnedPopMultiplier /= spawnPopulationMultiplier;
			SingletonDewNetworkBehaviour<Room>.instance.monsters.onAfterSpawn -= new Action<Entity>(OnAfterSpawn);
		}
	}

	private void OnAfterSpawn(Entity obj)
	{
		float size = UnityEngine.Random.Range(sizeMultiplier.x, sizeMultiplier.y);
		obj.EntityEvent_OnDeath += new Action<EventInfoKill>(OnWolfDeath);
		RpcApplyTransform(obj, size);
		obj.Control.outerRadius *= size;
		obj.Status.AddStatBonus(new StatBonus
		{
			maxHealthPercentage = bonusHealthPercentageBase + bonusHealthPercentagePerSize * (size - 1f)
		});
		void OnWolfDeath(EventInfoKill killObj)
		{
			if (((NetworkBehaviour)this).isServer && !(killObj.victim is Mon_LavaLand_SuperheatedWolf { didSelfDestruct: not false }))
			{
				CreateAbilityInstance(killObj.victim.agentPosition, null, new CastInfo(killObj.victim), (Ai_RoomMod_MeteoricLife_Explosion b) =>
				{
					b.Networksize = Mathf.Min(size, 1.35f);
				});
			}
		}
	}

	[ClientRpc]
	private void RpcApplyTransform(Entity ent, float scale)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)ent);
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, scale);
		((NetworkBehaviour)this).SendRPCInternal("System.Void RoomMod_MeteoricLife::RpcApplyTransform(Entity,System.Single)", -390082712, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcApplyTransform__Entity__Single(Entity ent, float scale)
	{
		ent.Visual.model.dissolveDelay = 3f;
		ent.Visual.model.dissolveDuration = 0f;
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
			((RoomMod_MeteoricLife)(object)obj).UserCode_RpcApplyTransform__Entity__Single(NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader), NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	static RoomMod_MeteoricLife()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(RoomMod_MeteoricLife), "System.Void RoomMod_MeteoricLife::RpcApplyTransform(Entity,System.Single)", (RemoteCallDelegate)InvokeUserCode_RpcApplyTransform__Entity__Single);
	}
}
