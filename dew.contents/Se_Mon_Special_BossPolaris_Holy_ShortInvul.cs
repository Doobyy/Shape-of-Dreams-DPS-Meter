using System;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Se_Mon_Special_BossPolaris_Holy_ShortInvul : StatusEffect
{
	public GameObject fxNegateDamage;

	public float duration = 2f;

	private float _lastTextPopupTime;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoArmorBoost(500f);
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			SetTimer(duration);
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (!(Time.time - _lastTextPopupTime < 0.25f))
		{
			_lastTextPopupTime = Time.time;
			FxPlayNewNetworked(fxNegateDamage, victim);
			RpcShowPopupText();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (bool)(UnityEngine.Object)(object)victim)
		{
			victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	[ClientRpc]
	private void RpcShowPopupText()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Se_Mon_Special_BossPolaris_Holy_ShortInvul::RpcShowPopupText()", -1367391895, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcShowPopupText()
	{
		InGameUIManager.instance.ShowWorldPopMessage(new WorldMessageSetting
		{
			rawText = DewLocalization.GetUIValue("Se_OntologicalShield_Popup"),
			color = new Color(0.865f, 1f, 0.45f),
			worldPosGetter = () => (!((UnityEngine.Object)(object)victim != null)) ? Vector3.zero : victim.Visual.GetCenterPosition()
		});
	}

	protected static void InvokeUserCode_RpcShowPopupText(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowPopupText called on server.");
		}
		else
		{
			((Se_Mon_Special_BossPolaris_Holy_ShortInvul)(object)obj).UserCode_RpcShowPopupText();
		}
	}

	static Se_Mon_Special_BossPolaris_Holy_ShortInvul()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Se_Mon_Special_BossPolaris_Holy_ShortInvul), "System.Void Se_Mon_Special_BossPolaris_Holy_ShortInvul::RpcShowPopupText()", (RemoteCallDelegate)InvokeUserCode_RpcShowPopupText);
	}
}
