using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_Mon_Special_BossObliviax_TurnAtk : AbilityInstance
{
	public float maxRotation;

	public float rotDuration;

	public float allowMarginDistance;

	public DewCollider range;

	public float angleDeviation;

	public ScalingValue dmgFactor;

	public Knockback knockback;

	public GameObject fxMain;

	public GameObject fxHit;

	public int projectileCount;

	public Vector2 projectileRange;

	private EntityTransformModifier _entTransform;

	private float _initialTime;

	private float _rotSpeed;

	[SyncVar]
	private bool _isComplete;

	public bool Network_isComplete
	{
		get
		{
			return _isComplete;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isComplete, 64uL, (Action<bool, bool>)null);
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		_entTransform = info.caster.Visual.GetNewTransformModifier();
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			FxPlayNewNetworked(fxHit, entity);
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.caster.agentPosition).Dispatch(entity);
			knockback.ApplyWithOrigin(info.caster.agentPosition, entity);
		}
		handle.Return();
		info.caster.Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle + 220f, immediately: false);
		_rotSpeed = maxRotation / rotDuration;
		RpcRotate(_rotSpeed);
		FxPlayNetworked(fxMain, info.caster);
		yield return new SI.WaitForSeconds(0.05f);
		projectileCount = Mathf.CeilToInt((float)projectileCount * NetworkedManagerBase<GameManager>.instance.GetSpecialSkillChanceMultiplier());
		if (projectileCount <= 8)
		{
			projectileCount = 8;
		}
		float initialAngle = UnityEngine.Random.Range(0f, 360f);
		int addedAngle = 360 / projectileCount;
		for (int j = 0; j < projectileCount; j++)
		{
			float y = initialAngle + (float)(addedAngle * j) + UnityEngine.Random.Range(0f - angleDeviation, angleDeviation);
			Vector3 vector = (Quaternion.Euler(0f, y, 0f) * info.forward).normalized * UnityEngine.Random.Range(projectileRange.x, projectileRange.y);
			Vector3 end = info.caster.agentPosition + vector;
			Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, end);
			if (!(end.sqrMagnitude - validAgentDestination_LinearSweep.sqrMagnitude >= allowMarginDistance))
			{
				CreateAbilityInstance<Ai_Mon_Special_BossObliviax_TurnAtk_Projectile>(validAgentDestination_LinearSweep, null, new CastInfo(info.caster, validAgentDestination_LinearSweep));
				yield return new SI.WaitForSeconds(0.05f);
			}
		}
		Destroy();
	}

	[ClientRpc]
	private void RpcRotate(float rotSpeed)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, rotSpeed);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_Special_BossObliviax_TurnAtk::RpcRotate(System.Single)", -194085383, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_entTransform != null)
		{
			_entTransform.Stop();
			_entTransform = null;
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcRotate__Single(float rotSpeed)
	{
		StartSequence(Routine());
		IEnumerator Routine()
		{
			float angle = 0f;
			for (float t = 0f; t < rotDuration; t += LogicUpdateManager.logicDeltaTime)
			{
				float num = t / rotDuration;
				angle += _rotSpeed * num;
				_entTransform.rotation = Quaternion.Euler(0f, 0f - angle, 0f);
				if (angle >= maxRotation)
				{
					break;
				}
				yield return null;
			}
			if (((NetworkBehaviour)this).isServer)
			{
				Network_isComplete = true;
			}
		}
	}

	protected static void InvokeUserCode_RpcRotate__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRotate called on server.");
		}
		else
		{
			((Ai_Mon_Special_BossObliviax_TurnAtk)(object)obj).UserCode_RpcRotate__Single(NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	static Ai_Mon_Special_BossObliviax_TurnAtk()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_Special_BossObliviax_TurnAtk), "System.Void Ai_Mon_Special_BossObliviax_TurnAtk::RpcRotate(System.Single)", (RemoteCallDelegate)InvokeUserCode_RpcRotate__Single);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, _isComplete);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isComplete);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isComplete, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isComplete, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
