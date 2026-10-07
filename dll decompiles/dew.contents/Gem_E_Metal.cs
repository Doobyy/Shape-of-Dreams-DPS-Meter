using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Gem_E_Metal : Gem
{
	public ScalingValue cooldownReduction;

	public GameObject reducedEffect;

	private KillTracker _tracker;

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			_tracker = newOwner.TrackKills(10f, KillCallback);
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			_tracker?.Stop();
		}
	}

	private void KillCallback(EventInfoKill obj)
	{
		if (!((Object)(object)skill == null))
		{
			float value = GetValue(cooldownReduction);
			ApplyCooldownReduction(skill, value);
			RpcPlayFeedback();
			NotifyUse();
		}
	}

	[ClientRpc]
	private void RpcPlayFeedback()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Gem_E_Metal::RpcPlayFeedback()", -1327080922, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPlayFeedback()
	{
		FxPlay(reducedEffect, owner);
	}

	protected static void InvokeUserCode_RpcPlayFeedback(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayFeedback called on server.");
		}
		else
		{
			((Gem_E_Metal)(object)obj).UserCode_RpcPlayFeedback();
		}
	}

	static Gem_E_Metal()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Gem_E_Metal), "System.Void Gem_E_Metal::RpcPlayFeedback()", (RemoteCallDelegate)InvokeUserCode_RpcPlayFeedback);
	}
}
