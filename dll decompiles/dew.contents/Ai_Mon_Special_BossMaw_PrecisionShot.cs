using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_Mon_Special_BossMaw_PrecisionShot : AbilityInstance
{
	public float postDelay;

	public float rotDuration;

	public float rotSpeed;

	public float maxRotSpeedMult;

	public float minRotSpeedMult;

	public float rotDistanceThreshold;

	public GameObject fxCast;

	public GameObject fxTelegraph;

	public GameObject fxTransform;

	public GameObject fxDeform;

	public DewAnimationClip startClip;

	public DewAnimationClip endClip;

	public EntityModel heroModel;

	[SyncVar]
	private float _desiredAngle;

	private float _cv;

	private float _predictionValue;

	private Entity _target;

	private Ai_Mon_Special_BossMaw_PrecisionShot_Projectile _prefab;

	public float Network_desiredAngle
	{
		get
		{
			return _desiredAngle;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _desiredAngle, 64uL, (Action<float, float>)null);
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		_prefab = DewResources.GetByType<Ai_Mon_Special_BossMaw_PrecisionShot_Projectile>(default(ResourceLoadSettings));
		ChangeModelLocal();
		yield return null;
		info.caster.Animation.PlayAbilityAnimation(startClip);
		FxPlayNetworked(fxCast, info.caster);
		info.caster.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
		if (_target.IsNullInactiveDeadOrKnockedOut())
		{
			_target = Dew.SelectBestWithScore((IList<DewPlayer>)DewPlayer.gamePlayers, (Func<DewPlayer, int, float>)((DewPlayer p, int _) =>
			{
				if (p.hero.IsNullInactiveDeadOrKnockedOut())
				{
					return -1000f;
				}
				return p.hero.Status.isUndetectableByNonAllies ? (-500f) : (0f - Vector3.Distance(info.caster.agentPosition, p.hero.GetAIAgentPosition(info.caster)));
			}), 0f, (DewRandom)null).hero;
		}
		if ((UnityEngine.Object)(object)_target != null)
		{
			Network_desiredAngle = CastInfo.GetAngle(_target.GetAIAgentPosition(info.caster) - info.caster.agentPosition);
		}
		else
		{
			Network_desiredAngle = info.caster.rotation.eulerAngles.y;
		}
		_predictionValue = UnityEngine.Random.Range(0.5f, 1f);
		FxPlayNetworked(fxTelegraph, info.caster);
		yield return new SI.WaitForSeconds(rotDuration);
		info.caster.Animation.PlayAbilityAnimation(endClip);
		info.caster.Control.StartDaze(postDelay);
		CreateAbilityInstance<Ai_Mon_Special_BossMaw_PrecisionShot_Projectile>(info.caster.agentPosition, null, new CastInfo(info.caster, rotation.eulerAngles.y));
		FxStopNetworked(fxTelegraph);
		FxStopNetworked(fxCast);
		yield return new SI.WaitForSeconds(postDelay);
		DestroyIfActive();
	}

	[ClientRpc]
	private void ChangeModelLocal()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_Special_BossMaw_PrecisionShot::ChangeModelLocal()", -1190820554, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	protected override void OnDestroyActor()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroyActor();
		info.caster.Visual.LoadModelDefaultLocal();
		FxPlay(fxDeform, info.caster);
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxTelegraph);
			FxStopNetworked(fxCast);
			if ((UnityEngine.Object)(object)info.caster != null)
			{
				info.caster.Control.DecrementBlockCounters(Channel.BlockedAction.Everything);
			}
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		float y = Mathf.SmoothDampAngle(rotation.eulerAngles.y, _desiredAngle, ref _cv, 0.1f);
		rotation = Quaternion.Euler(0f, y, 0f);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (_target.IsNullInactiveDeadOrKnockedOut())
		{
			_target = Dew.SelectBestWithScore((IList<DewPlayer>)DewPlayer.gamePlayers, (Func<DewPlayer, int, float>)((DewPlayer p, int _) =>
			{
				if (p.hero.IsNullInactiveDeadOrKnockedOut())
				{
					return -1000f;
				}
				return p.hero.Status.isUndetectableByNonAllies ? (-500f) : (0f - Vector3.Distance(info.caster.agentPosition, p.hero.GetAIAgentPosition(info.caster)));
			}), 0.2f, (DewRandom)null).hero;
		}
		if (!_target.IsNullInactiveDeadOrKnockedOut())
		{
			float target = AbilityTrigger.PredictAngle_SpeedAcceleration(info.caster, _predictionValue, _target, info.caster.agentPosition, 0f, _prefab.startInFrontDistance, _prefab.initialSpeed, _prefab.targetSpeed, _prefab.acceleration);
			float num = Vector3.Distance(_target.GetAIAgentPosition(info.caster), info.caster.agentPosition);
			float num2 = rotSpeed * Mathf.Lerp(maxRotSpeedMult, minRotSpeedMult, num / rotDistanceThreshold);
			Network_desiredAngle = Mathf.MoveTowardsAngle(_desiredAngle, target, num2 * dt);
			info.caster.Control.Rotate(rotation, immediately: false);
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_ChangeModelLocal()
	{
		FxPlay(fxTransform, info.caster);
		info.caster.Visual.LoadModelLocal(heroModel);
	}

	protected static void InvokeUserCode_ChangeModelLocal(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC ChangeModelLocal called on server.");
		}
		else
		{
			((Ai_Mon_Special_BossMaw_PrecisionShot)(object)obj).UserCode_ChangeModelLocal();
		}
	}

	static Ai_Mon_Special_BossMaw_PrecisionShot()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_Special_BossMaw_PrecisionShot), "System.Void Ai_Mon_Special_BossMaw_PrecisionShot::ChangeModelLocal()", (RemoteCallDelegate)InvokeUserCode_ChangeModelLocal);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, _desiredAngle);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _desiredAngle);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _desiredAngle, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _desiredAngle, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
