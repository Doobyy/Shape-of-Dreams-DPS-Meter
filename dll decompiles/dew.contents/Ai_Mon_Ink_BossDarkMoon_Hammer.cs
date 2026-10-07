using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_Hammer : AbilityInstance
{
	public float duration;

	public float postDelay;

	public float backOffsetDistance;

	public float castDuration;

	public DewAnimationClip castClip;

	public GameObject fxWeaponEnd;

	public float ascendTime;

	public float ascendHeight;

	public DewAnimationClip ascendClip;

	public GameObject fxAscend;

	public float descendTime;

	public DewAnimationClip descendClip;

	public DewAnimationClip endClip;

	public GameObject fxDescend;

	public GameObject fxAtk;

	public GameObject fxTelegraph;

	public GameObject fxHit;

	public ScalingValue dmgFactor;

	public KnockUpStrength knockUpStrength;

	public DewCollider range;

	[Space(15f)]
	public int rageInstanceWaveCount;

	public int rageInstanceDirCount;

	public float rageInstanceInterval;

	public float rageInstanceDistance;

	public GameObject fxRageTelergraph;

	public GameObject sfxRageInstance;

	private EntityTransformModifier _entTransform;

	private Vector3 _targetPoint;

	private Vector3 _targetDirection;

	private Vector3 _destination;

	private bool _isRage;

	private bool _didDisableRenderers;

	protected override IEnumerator OnCreateSequenced()
	{
		_entTransform = info.caster.Visual.GetNewTransformModifier();
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		_isRage = ((Mon_Ink_BossDarkMoon)info.caster)._isRage;
		CreateBasicEffect(info.caster, new UnstoppableEffect(), castDuration + duration + postDelay);
		info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = castDuration + duration + postDelay,
			isAttack = false,
			onCancel = DestroyIfActive
		});
		info.caster.Animation.PlayAbilityAnimation(castClip);
		Vector3 vector = AbilityTrigger.PredictPoint_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), info.target, duration + castDuration);
		_targetDirection = (info.caster.position - vector).normalized;
		_targetPoint = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, vector);
		_destination = _targetPoint + _targetDirection * backOffsetDistance;
		FxPlayNetworked(fxTelegraph, _targetPoint, Quaternion.LookRotation(_targetDirection));
		yield return new SI.WaitForSeconds(castDuration);
		CreateBasicEffect(info.caster, new InvulnerableEffect(), duration + descendTime);
		CreateBasicEffect(info.caster, new UncollidableEffect(), duration + descendTime);
		yield return null;
		RpcAscend();
		yield return new SI.WaitForSeconds(duration);
		RpcDescendAndFinish();
		yield return new SI.WaitForSeconds(descendTime);
		info.caster.Animation.PlayAbilityAnimation(endClip);
		info.caster.Control.StartDaze(postDelay);
		FxPlayNetworked(fxAtk, _targetPoint, null);
		range.transform.position = _targetPoint;
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.point).Dispatch(entity);
			entity.Visual.KnockUp(knockUpStrength, isFriendly: false);
			FxPlayNewNetworked(fxHit, entity);
		}
		handle.Return();
		if (_isRage)
		{
			Quaternion rot = Quaternion.LookRotation(((Component)(object)info.caster).transform.forward, Vector3.up);
			for (int j = 0; j < rageInstanceWaveCount; j++)
			{
				for (int k = 0; k < rageInstanceDirCount; k++)
				{
					float num = 360f / (float)rageInstanceDirCount;
					Vector3 point = _targetPoint + rot * (Quaternion.Euler(0f, num * (float)k, 0f) * (Vector3.forward * (3f + rageInstanceDistance * (float)j)));
					CreateAbilityInstance<Ai_Mon_Ink_BossDarkMoon_Hammer_RageInstance>(point, null, new CastInfo(info.caster, point));
				}
				FxPlayNewNetworked(sfxRageInstance, _targetPoint, null);
				yield return new SI.WaitForSeconds(rageInstanceInterval);
			}
		}
		FxStopNetworked(fxDescend);
		yield return new SI.WaitForSeconds(postDelay);
		FxStopNetworked(fxWeaponEnd);
		Destroy();
	}

	[ClientRpc]
	private void RpcAscend()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_Ink_BossDarkMoon_Hammer::RpcAscend()", -351178354, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcDescendAndFinish()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_Ink_BossDarkMoon_Hammer::RpcDescendAndFinish()", -1094146236, (NetworkWriter)(object)val, 0, true);
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
		if (_didDisableRenderers)
		{
			_didDisableRenderers = false;
			if (!info.caster.IsNullOrInactive())
			{
				info.caster.Visual.EnableRenderersLocal();
			}
		}
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxWeaponEnd);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (_entTransform != null)
		{
			_entTransform.Stop();
			_entTransform = null;
		}
		_didDisableRenderers = false;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcAscend()
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (((NetworkBehaviour)this).isServer)
			{
				info.caster.Animation.PlayAbilityAnimation(ascendClip);
				info.caster.Control.RotateTowards(_targetPoint, immediately: true);
				FxPlayNetworked(fxAscend, info.caster);
				if (_isRage)
				{
					FxPlayNetworked(fxRageTelergraph, _targetPoint, Quaternion.LookRotation(_targetDirection));
				}
			}
			for (float t = 0f; t < ascendTime; t += LogicUpdateManager.logicDeltaTime)
			{
				if (_entTransform == null)
				{
					yield break;
				}
				float num = t / ascendTime;
				_entTransform.worldOffset = Vector3.up * (num * ascendHeight);
				_entTransform.scaleMultiplier = Vector3.one * (1f - num);
				yield return null;
			}
			if (_entTransform != null)
			{
				_entTransform.worldOffset = Vector3.up * ascendHeight;
				_entTransform.scaleMultiplier = Vector3.zero;
				info.caster.Visual.DisableRenderersLocal();
				_didDisableRenderers = true;
				if (((NetworkBehaviour)this).isServer)
				{
					FxStopNetworked(startEffect);
				}
			}
		}
	}

	protected static void InvokeUserCode_RpcAscend(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAscend called on server.");
		}
		else
		{
			((Ai_Mon_Ink_BossDarkMoon_Hammer)(object)obj).UserCode_RpcAscend();
		}
	}

	protected void UserCode_RpcDescendAndFinish()
	{
		StartSequence(Routine());
		IEnumerator Routine()
		{
			if (((NetworkBehaviour)this).isServer)
			{
				info.caster.Control.Teleport(_destination);
				info.caster.Control.RotateTowards(_targetPoint, immediately: true);
				info.caster.Animation.PlayAbilityAnimation(descendClip);
				FxStopNetworked(fxAscend);
				FxPlayNetworked(fxDescend, info.caster);
				FxPlayNetworked(fxWeaponEnd, info.caster);
			}
			if (_didDisableRenderers)
			{
				info.caster.Visual.EnableRenderersLocal();
				_didDisableRenderers = false;
			}
			for (float t = 0f; t < descendTime; t += LogicUpdateManager.logicDeltaTime)
			{
				float num = t / descendTime;
				_entTransform.worldOffset = Vector3.up * ((1f - num) * ascendHeight);
				_entTransform.scaleMultiplier = Vector3.one * num;
				yield return null;
			}
			_entTransform.worldOffset = Vector3.zero;
			_entTransform.scaleMultiplier = Vector3.one;
		}
	}

	protected static void InvokeUserCode_RpcDescendAndFinish(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDescendAndFinish called on server.");
		}
		else
		{
			((Ai_Mon_Ink_BossDarkMoon_Hammer)(object)obj).UserCode_RpcDescendAndFinish();
		}
	}

	static Ai_Mon_Ink_BossDarkMoon_Hammer()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_Ink_BossDarkMoon_Hammer), "System.Void Ai_Mon_Ink_BossDarkMoon_Hammer::RpcAscend()", (RemoteCallDelegate)InvokeUserCode_RpcAscend);
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_Ink_BossDarkMoon_Hammer), "System.Void Ai_Mon_Ink_BossDarkMoon_Hammer::RpcDescendAndFinish()", (RemoteCallDelegate)InvokeUserCode_RpcDescendAndFinish);
	}
}
