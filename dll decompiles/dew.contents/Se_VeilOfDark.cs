using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Se_VeilOfDark : StatusEffect
{
	public float duration;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		SetTimer(duration);
		while (true)
		{
			yield return new SI.WaitForSeconds(0.35f);
			PopUpBlindMessage();
		}
	}

	[ClientRpc]
	private void PopUpBlindMessage()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Se_VeilOfDark::PopUpBlindMessage()", -1226419209, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_PopUpBlindMessage()
	{
		Entity v = victim;
		InGameUIManager.instance.ShowWorldPopMessage(new WorldMessageSetting
		{
			rawText = DewLocalization.GetUIValue("RoomMod_VeilOfDark_IngameUI"),
			color = new Color(0.85f, 0.3f, 0.2f),
			worldPosGetter = () => (!((Object)(object)v != null)) ? Vector3.zero : v.Visual.GetCenterPosition()
		});
	}

	protected static void InvokeUserCode_PopUpBlindMessage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC PopUpBlindMessage called on server.");
		}
		else
		{
			((Se_VeilOfDark)(object)obj).UserCode_PopUpBlindMessage();
		}
	}

	static Se_VeilOfDark()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Se_VeilOfDark), "System.Void Se_VeilOfDark::PopUpBlindMessage()", (RemoteCallDelegate)InvokeUserCode_PopUpBlindMessage);
	}
}
