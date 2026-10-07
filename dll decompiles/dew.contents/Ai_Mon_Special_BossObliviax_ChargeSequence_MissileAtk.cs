using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_Mon_Special_BossObliviax_ChargeSequence_MissileAtk : AbilityInstance
{
	private class Ad_ObliviaxMissile
	{
		public float time;
	}

	[HideInInspector]
	public Vector3 initPoint;

	[HideInInspector]
	public float duration;

	public float projectileSpeed;

	public float damageInterval;

	public ScalingValue dmgFactor;

	public float collisionRadius;

	public GameObject fxTargetEntity;

	public GameObject fxHit;

	public GameObject fxStart;

	private float _initTime;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		FxPlayNetworked(fxTargetEntity, info.target);
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_initTime = Time.time;
			info.target.AddData(new Ad_ObliviaxMissile
			{
				time = _initTime
			});
		}
		((Component)(object)this).transform.position = initPoint;
		FxPlay(fxStart);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		FxStop(fxStart);
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxTargetEntity);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (Time.time - _initTime >= duration)
		{
			DestroyIfActive();
		}
		SyncProjectilePosition(dt, ((Component)(object)this).transform.position, info.target.agentPosition);
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, ((Component)(object)this).transform.position, collisionRadius, tvDefaultHarmfulEffectTargets))
		{
			if (!item.IsNullInactiveDeadOrKnockedOut())
			{
				if (!item.HasData<Ad_ObliviaxMissile>())
				{
					item.AddData(new Ad_ObliviaxMissile
					{
						time = Time.time
					});
				}
				item.TryGetData<Ad_ObliviaxMissile>(out var data);
				if (!(Time.time - data.time <= damageInterval))
				{
					CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(((Component)(object)this).transform.position).Dispatch(item);
					FxPlayNewNetworked(fxHit, item);
					data.time = Time.time;
				}
			}
		}
		handle.Return();
	}

	[ClientRpc]
	private void SyncProjectilePosition(float dt, Vector3 startPos, Vector3 targetPos)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, dt);
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, startPos);
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, targetPos);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_Special_BossObliviax_ChargeSequence_MissileAtk::SyncProjectilePosition(System.Single,UnityEngine.Vector3,UnityEngine.Vector3)", -770393773, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_SyncProjectilePosition__Single__Vector3__Vector3(float dt, Vector3 startPos, Vector3 targetPos)
	{
		((Component)(object)this).transform.position = startPos + (targetPos - ((Component)(object)this).transform.position).Flattened().normalized * (dt * projectileSpeed);
	}

	protected static void InvokeUserCode_SyncProjectilePosition__Single__Vector3__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC SyncProjectilePosition called on server.");
		}
		else
		{
			((Ai_Mon_Special_BossObliviax_ChargeSequence_MissileAtk)(object)obj).UserCode_SyncProjectilePosition__Single__Vector3__Vector3(NetworkReaderExtensions.ReadFloat(reader), NetworkReaderExtensions.ReadVector3(reader), NetworkReaderExtensions.ReadVector3(reader));
		}
	}

	static Ai_Mon_Special_BossObliviax_ChargeSequence_MissileAtk()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_Special_BossObliviax_ChargeSequence_MissileAtk), "System.Void Ai_Mon_Special_BossObliviax_ChargeSequence_MissileAtk::SyncProjectilePosition(System.Single,UnityEngine.Vector3,UnityEngine.Vector3)", (RemoteCallDelegate)InvokeUserCode_SyncProjectilePosition__Single__Vector3__Vector3);
	}
}
