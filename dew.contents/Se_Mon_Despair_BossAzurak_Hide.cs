using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Se_Mon_Despair_BossAzurak_Hide : StatusEffect
{
	public GameObject fxBurrow;

	public GameObject fxUnburrow;

	public float burrowTime = 1f;

	public AnimationCurve burrowCurve;

	public float unburrowTime = 1f;

	public AnimationCurve unburrowCurve;

	public SafeAction onBurrowComplete;

	private EntityTransformModifier _mod;

	private bool _shouldEnableRenderers;

	private Channel _channel;

	protected override IEnumerator OnCreateSequenced()
	{
		_mod = victim.Visual.GetNewTransformModifier();
		if (((NetworkBehaviour)this).isServer)
		{
			_channel = victim.Control.StartChannel(new Channel
			{
				duration = float.PositiveInfinity,
				blockedActions = Channel.BlockedAction.Everything
			});
			RpcAnimateTransform(isBurrowing: true);
			FxPlayNetworked(fxBurrow, victim);
			yield return new SI.WaitForSeconds(burrowTime + 0.1f);
			FxStopNetworked(fxBurrow);
			DoInvulnerable();
			onBurrowComplete?.Invoke();
		}
	}

	[Server]
	public void UnburrowAndDestroy()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Se_Mon_Despair_BossAzurak_Hide::UnburrowAndDestroy()' called when server was not active");
			return;
		}
		FxPlayNetworked(fxUnburrow, victim);
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			RpcAnimateTransform(isBurrowing: false);
			yield return new WaitForSeconds(unburrowTime);
			FxStopNetworked(fxUnburrow);
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_shouldEnableRenderers)
		{
			victim.Visual.EnableRenderersLocal();
			_shouldEnableRenderers = false;
		}
		if (_mod != null)
		{
			_mod.Stop();
			_mod = null;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			if (_channel != null && _channel.isAlive)
			{
				_channel.Cancel();
				_channel = null;
			}
			FxStopNetworked(fxBurrow);
			FxStopNetworked(fxUnburrow);
		}
	}

	[ClientRpc]
	private void RpcAnimateTransform(bool isBurrowing)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, isBurrowing);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Se_Mon_Despair_BossAzurak_Hide::RpcAnimateTransform(System.Boolean)", 646102723, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcAnimateTransform__Boolean(bool isBurrowing)
	{
		AnimationCurve curve = (isBurrowing ? burrowCurve : unburrowCurve);
		float time = (isBurrowing ? burrowTime : unburrowTime);
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (_shouldEnableRenderers)
			{
				victim.Visual.EnableRenderersLocal();
				_shouldEnableRenderers = false;
			}
			for (float t = 0f; t < time; t += Time.deltaTime)
			{
				if (_mod == null)
				{
					yield break;
				}
				float time2 = t / time;
				float num = curve.Evaluate(time2);
				_mod.localOffset = Vector3.up * num;
				yield return null;
			}
			if (_mod != null)
			{
				_mod.localOffset = Vector3.up * curve.Evaluate(1f);
			}
			if (isBurrowing)
			{
				victim.Visual.DisableRenderersLocal();
				_shouldEnableRenderers = true;
			}
		}
	}

	protected static void InvokeUserCode_RpcAnimateTransform__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAnimateTransform called on server.");
		}
		else
		{
			((Se_Mon_Despair_BossAzurak_Hide)(object)obj).UserCode_RpcAnimateTransform__Boolean(NetworkReaderExtensions.ReadBool(reader));
		}
	}

	static Se_Mon_Despair_BossAzurak_Hide()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Se_Mon_Despair_BossAzurak_Hide), "System.Void Se_Mon_Despair_BossAzurak_Hide::RpcAnimateTransform(System.Boolean)", (RemoteCallDelegate)InvokeUserCode_RpcAnimateTransform__Boolean);
	}
}
