using System;
using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_Mon_Primus_BossPrimusAeron_Force_DropGiantSword : InstantDamageInstance
{
	public GameObject fxExplodePizza;

	public GameObject fxExplodeNonPizza;

	public AnimationCurve pizzaSwordYCurve;

	public Transform swordTransform;

	[NonSerialized]
	public Primus_Pizza0 pizza;

	[NonSerialized]
	private Vector3 _baseSwordLocalPos;

	[NonSerialized]
	private bool _hasBaseSwordLocalPos;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		if (swordTransform != null)
		{
			_baseSwordLocalPos = swordTransform.localPosition;
			_hasBaseSwordLocalPos = true;
		}
	}

	protected override void OnCreate()
	{
		position = info.point;
		rotation = UnityEngine.Random.rotation.Flattened();
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && !pizza.IsNullOrInactive())
		{
			pizza.PlayTelegraph();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		pizza = null;
		if (_hasBaseSwordLocalPos && swordTransform != null)
		{
			swordTransform.localPosition = _baseSwordLocalPos;
		}
	}

	protected override void OnAfterDelay()
	{
		base.OnAfterDelay();
		if (!pizza.IsNullOrInactive() && !pizza.isBroken)
		{
			FxPlayNetworked(fxExplodePizza);
			pizza.Break();
			RpcAnimateSwordOnPizza();
		}
		else
		{
			FxPlayNetworked(fxExplodeNonPizza);
		}
	}

	[ClientRpc]
	private void RpcAnimateSwordOnPizza()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_Primus_BossPrimusAeron_Force_DropGiantSword::RpcAnimateSwordOnPizza()", 135744252, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcAnimateSwordOnPizza()
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			Vector3 startPos = swordTransform.localPosition;
			for (float t = 0f; t < 5f; t += Time.deltaTime)
			{
				if (swordTransform == null)
				{
					break;
				}
				swordTransform.localPosition = startPos + Vector3.up * pizzaSwordYCurve.Evaluate(t);
				yield return null;
			}
		}
	}

	protected static void InvokeUserCode_RpcAnimateSwordOnPizza(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAnimateSwordOnPizza called on server.");
		}
		else
		{
			((Ai_Mon_Primus_BossPrimusAeron_Force_DropGiantSword)(object)obj).UserCode_RpcAnimateSwordOnPizza();
		}
	}

	static Ai_Mon_Primus_BossPrimusAeron_Force_DropGiantSword()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_Primus_BossPrimusAeron_Force_DropGiantSword), "System.Void Ai_Mon_Primus_BossPrimusAeron_Force_DropGiantSword::RpcAnimateSwordOnPizza()", (RemoteCallDelegate)InvokeUserCode_RpcAnimateSwordOnPizza);
	}
}
