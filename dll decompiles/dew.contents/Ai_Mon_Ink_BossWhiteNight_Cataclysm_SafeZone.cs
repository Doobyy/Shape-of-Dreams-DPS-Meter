using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_Mon_Ink_BossWhiteNight_Cataclysm_SafeZone : AbilityInstance
{
	public float checkInterval;

	public float endDelay;

	public GameObject fxShield;

	public GameObject adjustedTransform;

	public GameObject fxEnd;

	[SyncVar]
	internal float _radius;

	internal float _endTime;

	internal List<Vector3> _points;

	private float _lastCheckTime = float.NegativeInfinity;

	private FxGameObject _shieldEffect;

	public override bool reuseInRoom => true;

	public float Network_radius
	{
		get
		{
			return _radius;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _radius, 64uL, (Action<float, float>)null);
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		_shieldEffect = fxShield.GetComponent<FxGameObject>();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			_endTime += endDelay;
			for (int i = 0; i < _points.Count; i++)
			{
				Vector3 pos = _points[i];
				float duration = _endTime - Time.time;
				RpcPlayShieldEffect(pos, duration);
				yield return new SI.WaitForSeconds(0.25f);
			}
			yield return new SI.WaitForCondition(() => _endTime - Time.time < 0.001f);
			for (int num = 0; num < _points.Count; num++)
			{
				Vector3 pos2 = _points[num];
				RpcPlayEndEffect(pos2);
			}
			Destroy();
		}
	}

	[ClientRpc]
	private void RpcPlayShieldEffect(Vector3 pos, float duration)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, pos);
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, duration);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_Ink_BossWhiteNight_Cataclysm_SafeZone::RpcPlayShieldEffect(UnityEngine.Vector3,System.Single)", 31352839, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcPlayEndEffect(Vector3 pos)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, pos);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_Ink_BossWhiteNight_Cataclysm_SafeZone::RpcPlayEndEffect(UnityEngine.Vector3)", -588446872, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || _endTime - Time.time < 0.001f || Time.time - _lastCheckTime < checkInterval)
		{
			return;
		}
		_lastCheckTime = Time.time;
		List<Entity> list = DewPool.GetList(out ListReturnHandle<Entity> handle);
		for (int i = 0; i < _points.Count; i++)
		{
			Vector3 center = _points[i];
			List<Entity> collection = DewPhysics.OverlapCircleAllEntities(out var handle2, center, _radius, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
			{
				includeUncollidable = true
			});
			list.AddRange(collection);
			handle2.Return();
		}
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (allEntity.GetRelation(info.caster) != EntityRelation.Enemy)
			{
				continue;
			}
			Se_Mon_Ink_BossWhiteNight_Cataclysm_NotSafe effect2;
			if (list.Contains(allEntity))
			{
				if (allEntity.Status.TryGetStatusEffect<Se_Mon_Ink_BossWhiteNight_Cataclysm_NotSafe>(out var effect))
				{
					effect.Destroy();
				}
			}
			else if (!allEntity.Status.TryGetStatusEffect<Se_Mon_Ink_BossWhiteNight_Cataclysm_NotSafe>(out effect2))
			{
				CreateStatusEffect<Se_Mon_Ink_BossWhiteNight_Cataclysm_NotSafe>(allEntity, new CastInfo(info.caster, allEntity));
			}
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPlayShieldEffect__Vector3__Single(Vector3 pos, float duration)
	{
		_shieldEffect.sustainTime = duration;
		GameObject gameObject = FxPlayNew(fxShield, pos, Quaternion.identity);
		if (gameObject != null)
		{
			int siblingIndex = adjustedTransform.transform.GetSiblingIndex();
			gameObject.transform.GetChild(siblingIndex).localScale = Vector3.one * _radius;
		}
	}

	protected static void InvokeUserCode_RpcPlayShieldEffect__Vector3__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayShieldEffect called on server.");
		}
		else
		{
			((Ai_Mon_Ink_BossWhiteNight_Cataclysm_SafeZone)(object)obj).UserCode_RpcPlayShieldEffect__Vector3__Single(NetworkReaderExtensions.ReadVector3(reader), NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_RpcPlayEndEffect__Vector3(Vector3 pos)
	{
		fxEnd.transform.localScale = Vector3.one * _radius;
		FxPlayNew(fxEnd, pos, Quaternion.identity);
	}

	protected static void InvokeUserCode_RpcPlayEndEffect__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayEndEffect called on server.");
		}
		else
		{
			((Ai_Mon_Ink_BossWhiteNight_Cataclysm_SafeZone)(object)obj).UserCode_RpcPlayEndEffect__Vector3(NetworkReaderExtensions.ReadVector3(reader));
		}
	}

	static Ai_Mon_Ink_BossWhiteNight_Cataclysm_SafeZone()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_Ink_BossWhiteNight_Cataclysm_SafeZone), "System.Void Ai_Mon_Ink_BossWhiteNight_Cataclysm_SafeZone::RpcPlayShieldEffect(UnityEngine.Vector3,System.Single)", (RemoteCallDelegate)InvokeUserCode_RpcPlayShieldEffect__Vector3__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_Ink_BossWhiteNight_Cataclysm_SafeZone), "System.Void Ai_Mon_Ink_BossWhiteNight_Cataclysm_SafeZone::RpcPlayEndEffect(UnityEngine.Vector3)", (RemoteCallDelegate)InvokeUserCode_RpcPlayEndEffect__Vector3);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, _radius);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _radius);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _radius, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _radius, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
