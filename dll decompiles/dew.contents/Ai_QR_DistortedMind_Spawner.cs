using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_QR_DistortedMind_Spawner : AbilityInstance
{
	public ScalingValue countRaw;

	public ChargingChannelData channel;

	public GameObject fxShoot;

	public GameObject fxChargeSingle;

	public ParticleSystem psUpArrows;

	[NonSerialized]
	public int bonusOnFullCharge;

	[NonSerialized]
	public int countOffset;

	public SafeAction<int> onShoot;

	[SyncVar]
	private float _chargeAmount;

	private ChargingChannel _currentChannel;

	private int _lastCount;

	public int clampedCount => Mathf.Min(Mathf.RoundToInt(GetValue(countRaw)), 12) + countOffset;

	public override bool reuseInRoom => true;

	public float Network_chargeAmount
	{
		get
		{
			return _chargeAmount;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _chargeAmount, 64uL, (Action<float, float>)null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_currentChannel = channel.Get(this).OnCancel((ChargingChannel c) =>
			{
				ShootAndDestroy();
			}).OnCast((ChargingChannel c) =>
			{
				ShootAndDestroy();
			})
				.OnComplete((ChargingChannel c) =>
				{
					ShootAndDestroy();
				})
				.Dispatch(info.caster, firstTrigger);
			info.caster.Visual.genericStackIndicatorMax = clampedCount;
			info.caster.Visual.genericStackIndicatorValue = 0;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && _currentChannel != null)
		{
			Network_chargeAmount = _currentChannel.chargeAmount;
			int countFromNormalizedProgress = GetCountFromNormalizedProgress(_chargeAmount);
			info.caster.Visual.genericStackIndicatorValue = countFromNormalizedProgress;
			if (_lastCount != countFromNormalizedProgress)
			{
				_lastCount = countFromNormalizedProgress;
				FxPlayNetworked(fxChargeSingle, info.caster);
			}
		}
	}

	private int GetCountFromNormalizedProgress(float value)
	{
		return Mathf.FloorToInt(Mathf.Lerp(1f, clampedCount, value) + 0.01f);
	}

	[ClientRpc]
	private void PlayShootEffect(int count)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, count);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_QR_DistortedMind_Spawner::PlayShootEffect(System.Int32)", -672774316, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	private void ShootAndDestroy()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Ai_QR_DistortedMind_Spawner::ShootAndDestroy()' called when server was not active");
			return;
		}
		_currentChannel = null;
		StartSequence(Sequence());
		IEnumerator Sequence()
		{
			info.caster.Visual.genericStackIndicatorMax = 0;
			info.caster.Visual.genericStackIndicatorValue = 0;
			if (info.caster is Hero_Bismuth hero_Bismuth)
			{
				hero_Bismuth.SpendAttack();
				hero_Bismuth.book.RpcBookCast(hero_Bismuth.book.bookTransform.rotation.Flattened() * Quaternion.Euler(-70f, 0f, 0f));
			}
			int count = GetCountFromNormalizedProgress(_chargeAmount);
			if (count == clampedCount)
			{
				count += bonusOnFullCharge;
			}
			onShoot?.Invoke(count);
			Vector3 targetPoint = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, info.caster.owner.cursorWorldPos);
			PlayShootEffect(count);
			yield return new SI.WaitForSeconds(0.25f);
			List<Entity> targets = Hero_Bismuth.GetTargetEntities(out var handle, info.caster, canBeNeutral: true, 13f);
			for (int i = 0; i < count; i++)
			{
				if (targets.Count > 0)
				{
					Entity entity = targets[i % targets.Count];
					if ((UnityEngine.Object)(object)entity == null)
					{
						continue;
					}
					targetPoint = AbilityTrigger.PredictPoint_Simple(info.caster, UnityEngine.Random.value, entity, 0.1f);
				}
				Vector3 positionOnGround = Dew.GetPositionOnGround(targetPoint + UnityEngine.Random.insideUnitSphere * 1f);
				CreateAbilityInstance<Ai_QR_DistortedMind_Damage>(positionOnGround, null, new CastInfo(info.caster));
				yield return new SI.WaitForSeconds(0.1f);
			}
			handle.Return();
			Destroy();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		countOffset = 0;
		bonusOnFullCharge = 0;
		channel.canMove = true;
		_lastCount = 0;
		Network_chargeAmount = 0f;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_PlayShootEffect__Int32(int count)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if ((UnityEngine.Object)(object)psUpArrows != null)
		{
			EmissionModule emission = psUpArrows.emission;
			Burst burst = emission.GetBurst(0);
			burst.count = MinMaxCurve.op_Implicit((float)count);
			emission.SetBurst(0, burst);
		}
		FxPlay(fxShoot, info.caster);
	}

	protected static void InvokeUserCode_PlayShootEffect__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC PlayShootEffect called on server.");
		}
		else
		{
			((Ai_QR_DistortedMind_Spawner)(object)obj).UserCode_PlayShootEffect__Int32(NetworkReaderExtensions.ReadInt(reader));
		}
	}

	static Ai_QR_DistortedMind_Spawner()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_QR_DistortedMind_Spawner), "System.Void Ai_QR_DistortedMind_Spawner::PlayShootEffect(System.Int32)", (RemoteCallDelegate)InvokeUserCode_PlayShootEffect__Int32);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, _chargeAmount);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _chargeAmount);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _chargeAmount, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _chargeAmount, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
