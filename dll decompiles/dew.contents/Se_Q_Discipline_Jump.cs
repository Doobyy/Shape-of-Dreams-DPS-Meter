using System;
using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Se_Q_Discipline_Jump : StatusEffect
{
	public DewAnimationClip animPrepareNoJump;

	public DewAnimationClip animJump;

	public DewAnimationClip animStomp;

	public float gapDist = 2.5f;

	public float speed = 30f;

	public AnimationCurve jumpCurve;

	public float jumpDamageAmp = 2f;

	[NonSerialized]
	public bool enableBonusByDistance;

	private Vector3 _stompPos;

	private EntityTransformModifier _et;

	private float _strength = 1f;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		_strength = 1f;
		DoUnstoppable();
		DoUncollidable();
		_stompPos = (((UnityEngine.Object)(object)info.target == null) ? info.point : info.target.agentPosition);
		_stompPos = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, _stompPos);
		ResetCooldown(info.caster.Ability.attackAbility);
		if (Vector3.Distance(info.caster.agentPosition, _stompPos) < 3.5f)
		{
			info.caster.Animation.PlayAbilityAnimation(animPrepareNoJump);
			info.caster.Control.StartDaze(0.125f);
			yield return new SI.WaitForSeconds(0.1f);
			Stomp();
			yield break;
		}
		float range = firstTrigger.configs[0].castMethod.targetData.range;
		Vector3 vector = _stompPos - info.caster.agentPosition;
		float t = Mathf.Clamp01((vector.magnitude - 3.5f) / (range - 3.5f) * 1.15f);
		if (enableBonusByDistance)
		{
			_strength = Mathf.Lerp(1f, 1f + jumpDamageAmp, t);
		}
		info.caster.Animation.PlayAbilityAnimation(animJump);
		Vector3 destination = info.caster.agentPosition + vector.normalized * (vector.magnitude - gapDist);
		float duration = Mathf.Clamp(vector.magnitude / (speed * info.caster.Status.movementSpeedMultiplier), 0.1f, 0.5f);
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			destination = destination,
			duration = duration,
			ease = DewEase.EaseOutQuad,
			isFriendly = true,
			rotateForward = true,
			rotateSmoothly = false,
			canGoOverTerrain = true,
			affectedByMovementSpeed = false,
			isCanceledByCC = false,
			onCancel = DestroyIfActive,
			onFinish = Stomp
		});
		RpcStartTransform(duration);
	}

	[ClientRpc]
	private void RpcStartTransform(float duration)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, duration);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Se_Q_Discipline_Jump::RpcStartTransform(System.Single)", 781336865, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_et != null)
		{
			_et.Stop();
			_et = null;
		}
	}

	private void Stomp()
	{
		info.caster.Animation.PlayAbilityAnimation(animStomp);
		CreateAbilityInstance(_stompPos, Quaternion.LookRotation(_stompPos - info.caster.agentPosition).Flattened(), new CastInfo(info.caster), (Ai_Q_Discipline_Stomp ai) =>
		{
			ai.strengthMultiplier = _strength;
		});
		info.caster.Control.RotateTowards(_stompPos, immediately: true, 1f);
		info.caster.Control.StartDaze(0.125f);
		info.caster.Control.Attack(info.target, doChase: true);
		DestroyIfActive();
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcStartTransform__Single(float duration)
	{
		if (isActive)
		{
			_et = info.caster.Visual.GetNewTransformModifier();
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			float startTime = Time.time;
			while (Time.time - startTime < duration && _et != null)
			{
				_et.localOffset = Vector3.up * (jumpCurve.Evaluate((Time.time - startTime) / duration) * Mathf.Max(duration, 0.3f) * 7f);
				yield return null;
			}
		}
	}

	protected static void InvokeUserCode_RpcStartTransform__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStartTransform called on server.");
		}
		else
		{
			((Se_Q_Discipline_Jump)(object)obj).UserCode_RpcStartTransform__Single(NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	static Se_Q_Discipline_Jump()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Se_Q_Discipline_Jump), "System.Void Se_Q_Discipline_Jump::RpcStartTransform(System.Single)", (RemoteCallDelegate)InvokeUserCode_RpcStartTransform__Single);
	}
}
