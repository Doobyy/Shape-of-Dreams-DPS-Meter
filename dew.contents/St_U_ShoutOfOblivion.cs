using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class St_U_ShoutOfOblivion : SkillTrigger
{
	private ActorRef<StatusEffect> _invul;

	private CameraModifierOffset _offset;

	public override void OnCastStart(int configIndex, CastInfo info)
	{
		_invul = CreateBasicEffect(owner, new InvulnerableEffect(), 5f);
		RpcDoCameraOffset(info.forward);
		base.OnCastStart(configIndex, info);
	}

	protected override void OnCastCancel(int configIndex, CastInfo info)
	{
		if (!_invul.IsNullOrInactive())
		{
			_invul.Get().Destroy();
		}
		_invul = null;
		RpcRemoveCameraOffset();
		base.OnCastCancel(configIndex, info);
	}

	public override AbilityInstance OnCastComplete(int configIndex, CastInfo info)
	{
		if (!_invul.IsNullOrInactive())
		{
			_invul.Get().SetTimer(1f);
		}
		_invul = null;
		RpcRemoveCameraOffset();
		return base.OnCastComplete(configIndex, info);
	}

	[ClientRpc]
	private void RpcDoCameraOffset(Vector3 forward)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, forward);
		((NetworkBehaviour)this).SendRPCInternal("System.Void St_U_ShoutOfOblivion::RpcDoCameraOffset(UnityEngine.Vector3)", 1273861886, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcRemoveCameraOffset()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void St_U_ShoutOfOblivion::RpcRemoveCameraOffset()", -954287692, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		ForceClearOffset();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		ForceClearOffset();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_invul = null;
		ForceClearOffset();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ForceClearOffset();
	}

	private void ForceClearOffset()
	{
		if (_offset != null)
		{
			_offset.Remove();
			_offset = null;
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcDoCameraOffset__Vector3(Vector3 forward)
	{
		ForceClearOffset();
		if (((NetworkBehaviour)owner.owner).isLocalPlayer)
		{
			_offset = new CameraModifierOffset
			{
				offset = Vector3.zero
			}.Apply();
			CameraModifierOffset z = _offset;
			DOTween.Kill((object)z, false);
			TweenSettingsExtensions.SetId<TweenerCore<Vector3, Vector3, VectorOptions>>(DOTween.To((DOGetter<Vector3>)(() => z.offset), (DOSetter<Vector3>)((Vector3 x) =>
			{
				z.offset = x;
			}), forward * 5f, 0.75f), (object)z);
		}
	}

	protected static void InvokeUserCode_RpcDoCameraOffset__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDoCameraOffset called on server.");
		}
		else
		{
			((St_U_ShoutOfOblivion)(object)obj).UserCode_RpcDoCameraOffset__Vector3(NetworkReaderExtensions.ReadVector3(reader));
		}
	}

	protected void UserCode_RpcRemoveCameraOffset()
	{
		CameraModifierOffset z;
		if (_offset != null)
		{
			z = _offset;
			_offset = null;
			ManagerBase<CameraManager>.instance.StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			DOTween.Kill((object)z, false);
			TweenSettingsExtensions.SetId<TweenerCore<Vector3, Vector3, VectorOptions>>(DOTween.To((DOGetter<Vector3>)(() => z.offset), (DOSetter<Vector3>)((Vector3 x) =>
			{
				z.offset = x;
			}), Vector3.zero, 0.5f), (object)z);
			yield return new WaitForSeconds(0.5f);
			z.Remove();
		}
	}

	protected static void InvokeUserCode_RpcRemoveCameraOffset(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRemoveCameraOffset called on server.");
		}
		else
		{
			((St_U_ShoutOfOblivion)(object)obj).UserCode_RpcRemoveCameraOffset();
		}
	}

	static St_U_ShoutOfOblivion()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(St_U_ShoutOfOblivion), "System.Void St_U_ShoutOfOblivion::RpcDoCameraOffset(UnityEngine.Vector3)", (RemoteCallDelegate)InvokeUserCode_RpcDoCameraOffset__Vector3);
		RemoteProcedureCalls.RegisterRpc(typeof(St_U_ShoutOfOblivion), "System.Void St_U_ShoutOfOblivion::RpcRemoveCameraOffset()", (RemoteCallDelegate)InvokeUserCode_RpcRemoveCameraOffset);
	}
}
