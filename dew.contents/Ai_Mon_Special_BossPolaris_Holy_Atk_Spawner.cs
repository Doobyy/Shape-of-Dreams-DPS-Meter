using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_Mon_Special_BossPolaris_Holy_Atk_Spawner : AbilityInstance
{
	public GameObject fxTelegraphOnPoint;

	public GameObject fxThrow;

	public float prepareTime = 0.5f;

	public float throwInterval = 0.1f;

	public float afterThrowDelay;

	private Channel _channel;

	private Vector3 _telegraphOriginalSize;

	protected override IEnumerator OnCreateSequenced()
	{
		_telegraphOriginalSize = fxTelegraphOnPoint.transform.localScale;
		if ((bool)startEffect)
		{
			DewEffect.ApplySpeedMultiplier(startEffect, info.caster.Status.attackSpeedMultiplier / prepareTime);
		}
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		prepareTime /= info.caster.Status.attackSpeedMultiplier;
		throwInterval /= info.caster.Status.attackSpeedMultiplier;
		afterThrowDelay /= info.caster.Status.attackSpeedMultiplier;
		_channel = info.caster.Control.StartChannel(new Channel
		{
			blockedActions = (Channel.BlockedAction.Ability | Channel.BlockedAction.Attack),
			duration = 3600f,
			onCancel = DestroyIfActive
		});
		Ai_Mon_Special_BossPolaris_Holy_Atk_Projectile proj = DewResources.GetByType<Ai_Mon_Special_BossPolaris_Holy_Atk_Projectile>(ResourceLoadSettings.Light);
		List<(Vector3, float)> points = DewPool.GetList(out ListReturnHandle<(Vector3, float)> h);
		Mon_Special_BossPolaris mon_Special_BossPolaris = (Mon_Special_BossPolaris)info.caster;
		int throwCount = 3;
		if (mon_Special_BossPolaris.Holy_CanCastPhase2Abilities())
		{
			throwCount += 2;
		}
		if (mon_Special_BossPolaris.Holy_CanCastPhase3Abilities())
		{
			throwCount += 2;
		}
		for (int i = 0; i < throwCount; i++)
		{
			float predictionStrength = NetworkedManagerBase<GameManager>.instance.GetPredictionStrength();
			Vector3 end = AbilityTrigger.PredictPoint_SpeedAcceleration(info.caster, predictionStrength, info.target, info.caster.agentPosition, prepareTime + throwInterval * (float)i, proj.startInFrontDistance, proj.initialSpeed, proj.targetSpeed, proj.acceleration);
			end += Random.onUnitSphere.normalized.Flattened() * ((float)throwCount * 0.75f);
			end = Dew.GetValidAgentDestination_LinearSweep(info.target.GetAIAgentPosition(info.caster), end);
			float num = Mathf.Lerp(0.8f, 1.2f, Vector3.Distance(info.caster.agentPosition, end) / 13f);
			points.Add((end, num));
			RpcPlayTelegraph(end, num);
			yield return new SI.WaitForSeconds(throwInterval);
		}
		yield return new SI.WaitForSeconds(prepareTime - throwInterval * (float)throwCount);
		FxPlayNetworked(fxThrow, info.caster);
		int j;
		for (j = 0; j < throwCount; j++)
		{
			CreateAbilityInstance(position, null, new CastInfo(info.caster, points[j].Item1), (Ai_Mon_Special_BossPolaris_Holy_Atk_Projectile ai) =>
			{
				ai.NetworksizeMultiplier = points[j].Item2;
			});
			yield return new SI.WaitForSeconds(throwInterval);
		}
		h.Return();
		yield return new SI.WaitForSeconds(afterThrowDelay - throwInterval * (float)throwCount);
		Destroy();
	}

	[ClientRpc]
	private void RpcPlayTelegraph(Vector3 point, float sizeMult)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, point);
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, sizeMult);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_Special_BossPolaris_Holy_Atk_Spawner::RpcPlayTelegraph(UnityEngine.Vector3,System.Single)", 390531532, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _channel != null && _channel.isAlive)
		{
			_channel.Cancel();
			_channel = null;
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPlayTelegraph__Vector3__Single(Vector3 point, float sizeMult)
	{
		fxTelegraphOnPoint.transform.localScale = _telegraphOriginalSize * sizeMult;
		FxPlayNew(fxTelegraphOnPoint, point, null);
	}

	protected static void InvokeUserCode_RpcPlayTelegraph__Vector3__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayTelegraph called on server.");
		}
		else
		{
			((Ai_Mon_Special_BossPolaris_Holy_Atk_Spawner)(object)obj).UserCode_RpcPlayTelegraph__Vector3__Single(NetworkReaderExtensions.ReadVector3(reader), NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	static Ai_Mon_Special_BossPolaris_Holy_Atk_Spawner()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_Special_BossPolaris_Holy_Atk_Spawner), "System.Void Ai_Mon_Special_BossPolaris_Holy_Atk_Spawner::RpcPlayTelegraph(UnityEngine.Vector3,System.Single)", (RemoteCallDelegate)InvokeUserCode_RpcPlayTelegraph__Vector3__Single);
	}
}
