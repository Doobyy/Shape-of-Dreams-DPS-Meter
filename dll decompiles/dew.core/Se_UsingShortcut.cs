using System;
using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Se_UsingShortcut : StatusEffect
{
	public float speedMultiplier;

	public float maxFadeDuration;

	public float delay;

	public GameObject dissolveEffect;

	[NonSerialized]
	public Room_Shortcut startShortcut;

	[NonSerialized]
	public Room_Shortcut targetShortcut;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DoInvulnerable();
			DoUncollidable();
			DoUntargetable();
			DoInvisible(ignoreReveal: true);
			victim.Control.forceWalking = true;
			float speed = victim.Control.baseAgentSpeed * speedMultiplier;
			float num = Vector2.Distance(victim.position.ToXY(), startShortcut.walkStartPos.position.ToXY()) / speed;
			victim.Control.StartDaze(num);
			victim.Control.StartDisplacement(new DispByDestination
			{
				destination = startShortcut.walkStartPos.transform.position,
				canGoOverTerrain = true,
				duration = num,
				ease = DewEase.Linear,
				isFriendly = true,
				rotateForward = true,
				isCanceledByCC = false,
				rotateSmoothly = true
			});
			yield return new SI.WaitForSeconds(num);
			float num2 = Vector2.Distance(victim.position.ToXY(), startShortcut.walkEndPos.position.ToXY()) / speed;
			victim.Control.StartDaze(num2);
			victim.Control.StartDisplacement(new DispByDestination
			{
				destination = startShortcut.walkEndPos.transform.position,
				canGoOverTerrain = true,
				duration = num2,
				ease = DewEase.Linear,
				isFriendly = true,
				rotateForward = true,
				isCanceledByCC = false,
				rotateSmoothly = true
			});
			FxPlayNetworked(dissolveEffect, victim);
			TpcSetFadeStatus(victim.owner, value: true);
			TpcSetTransitionStatus(victim.owner, value: true);
			yield return new SI.WaitForSeconds(Mathf.Min(maxFadeDuration, num2));
			victim.Control.StartDaze(delay);
			yield return new SI.WaitForSeconds(delay * 0.5f);
			Teleport(victim, targetShortcut.walkEndPos.position);
			TpcSetCameraPos(victim.owner, targetShortcut.walkStartPos.position);
			targetShortcut.Open();
			yield return new SI.WaitForSeconds(delay * 0.5f);
			FxStopNetworked(dissolveEffect);
			TpcSetFadeStatus(victim.owner, value: false);
			float num3 = Vector2.Distance(victim.position.ToXY(), targetShortcut.walkStartPos.position.ToXY()) / speed;
			victim.Control.StartDaze(num3);
			victim.Control.StartDisplacement(new DispByDestination
			{
				destination = targetShortcut.walkStartPos.position,
				canGoOverTerrain = true,
				duration = num3,
				ease = DewEase.Linear,
				isFriendly = true,
				rotateForward = true,
				isCanceledByCC = false,
				rotateSmoothly = true
			});
			yield return new SI.WaitForSeconds(num3);
			victim.Control.forceWalking = false;
			TpcSetTransitionStatus(victim.owner, value: false);
			Destroy();
		}
	}

	[TargetRpc]
	private void TpcSetCameraPos(NetworkConnectionToClient target, Vector3 pos)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, pos);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)target, "System.Void Se_UsingShortcut::TpcSetCameraPos(Mirror.NetworkConnectionToClient,UnityEngine.Vector3)", -507826236, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[TargetRpc]
	private void TpcSetFadeStatus(NetworkConnectionToClient target, bool value)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, value);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)target, "System.Void Se_UsingShortcut::TpcSetFadeStatus(Mirror.NetworkConnectionToClient,System.Boolean)", -1238627915, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[TargetRpc]
	private void TpcSetTransitionStatus(NetworkConnectionToClient target, bool value)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, value);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)target, "System.Void Se_UsingShortcut::TpcSetTransitionStatus(Mirror.NetworkConnectionToClient,System.Boolean)", 1374971836, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			TpcSetTransitionStatus(victim.owner, value: false);
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TpcSetCameraPos__NetworkConnectionToClient__Vector3(NetworkConnectionToClient target, Vector3 pos)
	{
		ManagerBase<CameraManager>.instance.SetCameraPosition(pos);
	}

	protected static void InvokeUserCode_TpcSetCameraPos__NetworkConnectionToClient__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcSetCameraPos called on server.");
		}
		else
		{
			((Se_UsingShortcut)(object)obj).UserCode_TpcSetCameraPos__NetworkConnectionToClient__Vector3((NetworkConnectionToClient)(object)NetworkClient.connection, NetworkReaderExtensions.ReadVector3(reader));
		}
	}

	protected void UserCode_TpcSetFadeStatus__NetworkConnectionToClient__Boolean(NetworkConnectionToClient target, bool value)
	{
		if (value)
		{
			ManagerBase<TransitionManager>.instance.FadeOut(showTips: false);
		}
		else
		{
			ManagerBase<TransitionManager>.instance.FadeIn();
		}
	}

	protected static void InvokeUserCode_TpcSetFadeStatus__NetworkConnectionToClient__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcSetFadeStatus called on server.");
		}
		else
		{
			((Se_UsingShortcut)(object)obj).UserCode_TpcSetFadeStatus__NetworkConnectionToClient__Boolean((NetworkConnectionToClient)(object)NetworkClient.connection, NetworkReaderExtensions.ReadBool(reader));
		}
	}

	protected void UserCode_TpcSetTransitionStatus__NetworkConnectionToClient__Boolean(NetworkConnectionToClient target, bool value)
	{
		NetworkedManagerBase<ZoneManager>.instance.isInLocalTransition = value;
	}

	protected static void InvokeUserCode_TpcSetTransitionStatus__NetworkConnectionToClient__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcSetTransitionStatus called on server.");
		}
		else
		{
			((Se_UsingShortcut)(object)obj).UserCode_TpcSetTransitionStatus__NetworkConnectionToClient__Boolean((NetworkConnectionToClient)(object)NetworkClient.connection, NetworkReaderExtensions.ReadBool(reader));
		}
	}

	static Se_UsingShortcut()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Se_UsingShortcut), "System.Void Se_UsingShortcut::TpcSetCameraPos(Mirror.NetworkConnectionToClient,UnityEngine.Vector3)", (RemoteCallDelegate)InvokeUserCode_TpcSetCameraPos__NetworkConnectionToClient__Vector3);
		RemoteProcedureCalls.RegisterRpc(typeof(Se_UsingShortcut), "System.Void Se_UsingShortcut::TpcSetFadeStatus(Mirror.NetworkConnectionToClient,System.Boolean)", (RemoteCallDelegate)InvokeUserCode_TpcSetFadeStatus__NetworkConnectionToClient__Boolean);
		RemoteProcedureCalls.RegisterRpc(typeof(Se_UsingShortcut), "System.Void Se_UsingShortcut::TpcSetTransitionStatus(Mirror.NetworkConnectionToClient,System.Boolean)", (RemoteCallDelegate)InvokeUserCode_TpcSetTransitionStatus__NetworkConnectionToClient__Boolean);
	}
}
