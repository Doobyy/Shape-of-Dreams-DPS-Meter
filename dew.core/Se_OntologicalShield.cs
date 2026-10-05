using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Se_OntologicalShield : StatusEffect
{
	public GameObject fxNegateDamage;

	private float _lastTextPopupTime;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoInvulnerable((EventInfoDamageNegatedByImmunity i) =>
		{
			if (!(Time.time - _lastTextPopupTime < 0.25f))
			{
				_lastTextPopupTime = Time.time;
				FxPlayNetworked(fxNegateDamage, victim);
				RpcShowPopupText();
			}
		});
	}

	[ClientRpc]
	private void RpcShowPopupText()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Se_OntologicalShield::RpcShowPopupText()", -2040554841, (NetworkWriter)(object)val, 0, true);
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
			worldPosGetter = () => (!((Object)(object)victim != null)) ? Vector3.zero : victim.Visual.GetCenterPosition()
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
			((Se_OntologicalShield)(object)obj).UserCode_RpcShowPopupText();
		}
	}

	static Se_OntologicalShield()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Se_OntologicalShield), "System.Void Se_OntologicalShield::RpcShowPopupText()", (RemoteCallDelegate)InvokeUserCode_RpcShowPopupText);
	}
}
