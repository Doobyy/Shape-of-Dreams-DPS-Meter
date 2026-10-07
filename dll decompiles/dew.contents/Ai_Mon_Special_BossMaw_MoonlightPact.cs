using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_Mon_Special_BossMaw_MoonlightPact : AbilityInstance
{
	public float summonDelay;

	public float postDelay;

	public GameObject fxSummon;

	public DewAnimationClip summonClip;

	public GameObject fxTransform;

	public EntityModel heroModel;

	private Sum_BossMaw_MoonlightPact_Fenrir _summon;

	private bool _isDefaultModel = true;

	private Channel _channel;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			CreateBasicEffect(info.caster, new UnstoppableEffect(), summonDelay + postDelay);
			RpcChangeModel();
			yield return null;
			_channel = info.caster.Control.StartChannel(new Channel
			{
				blockedActions = Channel.BlockedAction.Everything,
				duration = float.PositiveInfinity
			});
			yield return new SI.WaitForSeconds(summonDelay);
			info.caster.Animation.PlayAbilityAnimation(summonClip);
			FxPlayNetworked(fxSummon, info.caster);
			_summon = SpawnSummon<Sum_BossMaw_MoonlightPact_Fenrir>(Dew.GetGoodRewardPosition(info.caster.agentPosition), Quaternion.Euler(0f, Random.Range(0, 360), 0f));
			DestroyOnDeath(_summon);
			yield return new SI.WaitForSeconds(postDelay);
			RpcDeformModel();
			if (_channel != null && _channel.isAlive)
			{
				_channel.Cancel();
				_channel = null;
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!_isDefaultModel)
		{
			FxPlay(fxTransform, info.caster);
			info.caster.Visual.LoadModelDefaultLocal();
		}
		if (((NetworkBehaviour)this).isServer)
		{
			if ((Object)(object)_summon != null)
			{
				_summon.Destroy();
				_summon = null;
			}
			if (_channel != null && _channel.isAlive)
			{
				_channel.Cancel();
				_channel = null;
			}
		}
	}

	[ClientRpc]
	private void RpcChangeModel()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_Special_BossMaw_MoonlightPact::RpcChangeModel()", -1545227721, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcDeformModel()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_Special_BossMaw_MoonlightPact::RpcDeformModel()", -166796414, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcChangeModel()
	{
		FxPlay(fxTransform, info.caster);
		info.caster.Visual.LoadModelLocal(heroModel);
		_isDefaultModel = false;
	}

	protected static void InvokeUserCode_RpcChangeModel(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcChangeModel called on server.");
		}
		else
		{
			((Ai_Mon_Special_BossMaw_MoonlightPact)(object)obj).UserCode_RpcChangeModel();
		}
	}

	protected void UserCode_RpcDeformModel()
	{
		FxPlay(fxTransform, info.caster);
		info.caster.Visual.LoadModelDefaultLocal();
		_isDefaultModel = true;
	}

	protected static void InvokeUserCode_RpcDeformModel(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDeformModel called on server.");
		}
		else
		{
			((Ai_Mon_Special_BossMaw_MoonlightPact)(object)obj).UserCode_RpcDeformModel();
		}
	}

	static Ai_Mon_Special_BossMaw_MoonlightPact()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_Special_BossMaw_MoonlightPact), "System.Void Ai_Mon_Special_BossMaw_MoonlightPact::RpcChangeModel()", (RemoteCallDelegate)InvokeUserCode_RpcChangeModel);
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_Special_BossMaw_MoonlightPact), "System.Void Ai_Mon_Special_BossMaw_MoonlightPact::RpcDeformModel()", (RemoteCallDelegate)InvokeUserCode_RpcDeformModel);
	}
}
