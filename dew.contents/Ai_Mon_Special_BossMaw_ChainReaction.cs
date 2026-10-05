using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_Mon_Special_BossMaw_ChainReaction : AbilityInstance
{
	public float postDelay;

	public float startDelay;

	public float castDuration;

	public float deviation;

	public float maxRange;

	public EntityModel heroModel;

	public DewAnimationClip castClip;

	public DewAnimationClip endClip;

	public GameObject fxCast;

	public GameObject fxTelegraph;

	public GameObject fxTransform;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
			info.caster.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
			ChangeModelLocal();
			yield return new SI.WaitForSeconds(startDelay);
			Vector3 point = info.caster.agentPosition;
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.caster.agentPosition, maxRange, tvDefaultHarmfulEffectTargets);
			if (list.Count > 0)
			{
				point = AbilityTrigger.PredictPoint_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), list[0], castDuration);
			}
			handle.Return();
			point = Dew.GetValidAgentDestination_Closest(end: point + Random.insideUnitCircle.ToXZ().normalized * Random.Range(0f, deviation), start: info.caster.agentPosition);
			info.caster.Animation.PlayAbilityAnimation(castClip);
			FxPlayNetworked(fxCast, info.caster);
			FxPlayNetworked(fxTelegraph, point, Quaternion.identity);
			yield return new SI.WaitForSeconds(castDuration);
			FxStopNetworked(fxCast);
			info.caster.Animation.PlayAbilityAnimation(endClip);
			info.caster.Control.StartDaze(postDelay);
			CreateAbilityInstance<Ai_Mon_Special_BossMaw_ChainReaction_Instance>(point, null, new CastInfo(info.caster, point));
			yield return new SI.WaitForSeconds(postDelay);
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroyActor();
		FxPlay(fxTransform, info.caster);
		info.caster.Visual.LoadModelDefaultLocal();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxCast);
			info.caster.Control.DecrementBlockCounters(Channel.BlockedAction.Everything);
		}
	}

	[ClientRpc]
	private void ChangeModelLocal()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_Special_BossMaw_ChainReaction::ChangeModelLocal()", 1154090920, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
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
			((Ai_Mon_Special_BossMaw_ChainReaction)(object)obj).UserCode_ChangeModelLocal();
		}
	}

	static Ai_Mon_Special_BossMaw_ChainReaction()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_Special_BossMaw_ChainReaction), "System.Void Ai_Mon_Special_BossMaw_ChainReaction::ChangeModelLocal()", (RemoteCallDelegate)InvokeUserCode_ChangeModelLocal);
	}
}
