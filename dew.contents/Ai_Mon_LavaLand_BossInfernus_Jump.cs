using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_Mon_LavaLand_BossInfernus_Jump : AbilityInstance
{
	public float ascendTime;

	public float ascendHeight;

	public DewAnimationClip animAscend;

	public float beforeTelegraphDelay;

	public float telegraphTime;

	public float landPosRandomMag = 3f;

	public GameObject fxTelegraph;

	public DewAnimationClip animDescendFalling;

	public DewAnimationClip animLand;

	public float descendTime;

	public int landMeteorCount;

	public float postDaze;

	public DewAnimationClip animLandOnAlt;

	private EntityTransformModifier _entTransform;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		_entTransform = info.caster.Visual.GetNewTransformModifier();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			CreateStatusEffect<Se_Mon_LavaLand_BossInfernus_Jump_Invul>(info.caster).DestroyOnDestroy(this);
			info.caster.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
			RpcAscend();
			yield return new SI.WaitForSeconds(ascendTime + beforeTelegraphDelay);
			Vector3 aIAgentPosition = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true).GetAIAgentPosition(info.caster);
			Vector3 positionOnGround = Dew.GetPositionOnGround(aIAgentPosition + Random.onUnitSphere * landPosRandomMag);
			positionOnGround = Dew.GetValidAgentDestination_Closest(aIAgentPosition, positionOnGround);
			FxPlayNetworked(fxTelegraph, positionOnGround, Quaternion.identity);
			Teleport(info.caster, positionOnGround);
			yield return new SI.WaitForSeconds(telegraphTime);
			RpcDescendAndFinish();
		}
	}

	protected override void OnDestroyActor()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroyActor();
		if (_entTransform != null)
		{
			_entTransform.Stop();
			_entTransform = null;
		}
		if (((NetworkBehaviour)this).isServer && !info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			info.caster.Control.DecrementBlockCounters(Channel.BlockedAction.Everything);
			info.caster.Control.StartDaze(postDaze);
		}
	}

	[ClientRpc]
	private void RpcAscend()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_LavaLand_BossInfernus_Jump::RpcAscend()", 487216220, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcDescendAndFinish()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_LavaLand_BossInfernus_Jump::RpcDescendAndFinish()", -1869160302, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcAscend()
	{
		StartSequence(Routine());
		IEnumerator Routine()
		{
			if (((NetworkBehaviour)this).isServer)
			{
				info.caster.Animation.PlayAbilityAnimation(animAscend);
			}
			for (float t = 0f; t < ascendTime; t += LogicUpdateManager.logicDeltaTime)
			{
				float num = t / ascendTime;
				_entTransform.worldOffset = Vector3.up * num * ascendHeight;
				_entTransform.scaleMultiplier = Vector3.one * (1f - num);
				yield return null;
			}
			_entTransform.worldOffset = Vector3.up * ascendHeight;
			_entTransform.scaleMultiplier = Vector3.zero;
			info.caster.Visual.DisableRenderersLocal();
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
			((Ai_Mon_LavaLand_BossInfernus_Jump)(object)obj).UserCode_RpcAscend();
		}
	}

	protected void UserCode_RpcDescendAndFinish()
	{
		StartSequence(Routine());
		IEnumerator Routine()
		{
			if (((NetworkBehaviour)this).isServer)
			{
				info.caster.Animation.PlayAbilityAnimation(animDescendFalling);
				info.caster.Control.RotateTowards(Dew.GetClosestAliveHero(info.caster.agentPosition, fallbackToDead: true, info.caster), immediately: true);
			}
			info.caster.Visual.EnableRenderersLocal();
			for (float t = 0f; t < descendTime; t += LogicUpdateManager.logicDeltaTime)
			{
				float num = t / descendTime;
				_entTransform.worldOffset = Vector3.up * ((1f - num) * ascendHeight);
				_entTransform.scaleMultiplier = Vector3.one * num;
				yield return null;
			}
			_entTransform.worldOffset = Vector3.zero;
			_entTransform.scaleMultiplier = Vector3.one;
			if (((NetworkBehaviour)this).isServer)
			{
				bool flag = true;
				DewAnimationClip clip = animLand;
				if (((Monster)info.caster).currentPoolIndex == 1 && Random.value < 0.5f)
				{
					clip = animLandOnAlt;
					flag = false;
				}
				info.caster.Animation.PlayAbilityAnimation(clip);
				CreateAbilityInstance<Ai_Mon_LavaLand_BossInfernus_Jump_Land>(info.caster.agentPosition, null, new CastInfo(info.caster));
				if (flag)
				{
					CreateAbilityInstance(info.caster.agentPosition, null, new CastInfo(info.caster), (Ai_Mon_LavaLand_BossInfernus_MeteorSpawner ai) =>
					{
						ai.count = landMeteorCount;
					});
				}
				else
				{
					CreateAbilityInstance<Ai_Mon_LavaLand_BossInfernus_Roar>(info.caster.agentPosition, null, new CastInfo(info.caster));
				}
				info.caster.Control.StartDaze(postDaze);
				Destroy();
			}
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
			((Ai_Mon_LavaLand_BossInfernus_Jump)(object)obj).UserCode_RpcDescendAndFinish();
		}
	}

	static Ai_Mon_LavaLand_BossInfernus_Jump()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_LavaLand_BossInfernus_Jump), "System.Void Ai_Mon_LavaLand_BossInfernus_Jump::RpcAscend()", (RemoteCallDelegate)InvokeUserCode_RpcAscend);
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_LavaLand_BossInfernus_Jump), "System.Void Ai_Mon_LavaLand_BossInfernus_Jump::RpcDescendAndFinish()", (RemoteCallDelegate)InvokeUserCode_RpcDescendAndFinish);
	}
}
