using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Shrine_BrokenWoodenDoll : Shrine
{
	public SafeAction ClientEvent_OnDollReactivated;

	public float soulCompleteDelay = 0.5f;

	public GameObject fxSoulComplete;

	public float disappearDelay = 2f;

	public GameObject fxDisappear;

	public int requiredSouls = 3;

	[SaveVar(SaveVarFlags.Default)]
	[SyncVar]
	private int _givenSoul;

	public int Network_givenSoul
	{
		get
		{
			return _givenSoul;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref _givenSoul, 256uL, (Action<int, int>)null);
		}
	}

	public override bool CanInteract(Entity entity)
	{
		if (base.CanInteract(entity) && entity.Status.HasStatusEffect<Se_Shrine_SmallSoul>())
		{
			return _givenSoul < requiredSouls;
		}
		return false;
	}

	protected override bool OnUse(Entity entity)
	{
		if (!entity.Status.TryGetStatusEffect<Se_Shrine_SmallSoul>(out var effect))
		{
			return false;
		}
		Network_givenSoul = _givenSoul + 1;
		effect.RemoveStack();
		((MonoBehaviour)(object)this).StartCoroutine(DropStardustRoutine());
		if (_givenSoul >= requiredSouls)
		{
			RpcNotifyReactivation();
			((MonoBehaviour)(object)this).StartCoroutine(FinishRoutine());
		}
		return true;
		IEnumerator DropStardustRoutine()
		{
			yield return new WaitForSeconds(0.75f);
			NetworkedManagerBase<PickupManager>.instance.DropStarDust(3, Dew.GetGoodRewardPosition(position + ((Component)(object)this).transform.forward * 1.5f));
		}
		IEnumerator FinishRoutine()
		{
			yield return new WaitForSeconds(soulCompleteDelay);
			FxPlayNetworked(fxSoulComplete);
			yield return new WaitForSeconds(disappearDelay);
			FxPlayNetworked(fxDisappear);
			Destroy();
		}
	}

	[ClientRpc]
	private void RpcNotifyReactivation()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Shrine_BrokenWoodenDoll::RpcNotifyReactivation()", 623707530, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcNotifyReactivation()
	{
		ClientEvent_OnDollReactivated?.Invoke();
	}

	protected static void InvokeUserCode_RpcNotifyReactivation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcNotifyReactivation called on server.");
		}
		else
		{
			((Shrine_BrokenWoodenDoll)(object)obj).UserCode_RpcNotifyReactivation();
		}
	}

	static Shrine_BrokenWoodenDoll()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_BrokenWoodenDoll), "System.Void Shrine_BrokenWoodenDoll::RpcNotifyReactivation()", (RemoteCallDelegate)InvokeUserCode_RpcNotifyReactivation);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteInt(writer, _givenSoul);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, _givenSoul);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _givenSoul, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _givenSoul, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
	}
}
