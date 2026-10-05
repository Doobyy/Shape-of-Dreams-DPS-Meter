using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Se_ObliviaxKidnap : StatusEffect
{
	public Transform tentacleTransform;

	public Transform tentacleTip;

	public AnimationCurve yOffsetCurve;

	public float duration;

	private EntityTransformModifier _modifier;

	private bool _didDisableRenderer;

	private bool _didDisableCharacterControls;

	public override bool isDestroyedOnRoomChange => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		ManagerBase<MusicManager>.instance.Stop();
		tentacleTransform.localPosition = new Vector3(0f, yOffsetCurve.Evaluate(0f) - 5f, 0f);
		if (((NetworkBehaviour)victim).isOwned)
		{
			ManagerBase<ControlManager>.instance.DisableCharacterControls();
			_didDisableCharacterControls = true;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			DoProtected(null);
			DoUntargetable();
			DoUncollidable();
			DoRoot();
			DoStun();
			DoSilence();
			victim.Control.Stop();
			victim.Control.CancelOngoingChannels();
			RpcPlayAnimation();
			victim.Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle + 180f + (float)Random.Range(-60, 60), immediately: true);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_modifier != null)
		{
			_modifier.Stop();
			_modifier = null;
		}
		if (_didDisableRenderer)
		{
			victim.Visual.EnableRenderersLocal();
			_didDisableRenderer = false;
		}
		if (_didDisableCharacterControls && ManagerBase<ControlManager>.instance != null)
		{
			ManagerBase<ControlManager>.instance.EnableCharacterControls();
			_didDisableCharacterControls = false;
		}
		if ((Object)(object)NetworkedManagerBase<ZoneManager>.instance != null && !NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition && (Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null)
		{
			ManagerBase<MusicManager>.instance.Play(SingletonDewNetworkBehaviour<Room>.instance.music);
		}
	}

	[ClientRpc]
	private void RpcPlayAnimation()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Se_ObliviaxKidnap::RpcPlayAnimation()", -1061791930, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void LateUpdate()
	{
		if (_modifier != null && tentacleTip != null)
		{
			_modifier.rotation = Quaternion.Inverse(victim.rotation) * tentacleTip.rotation;
			_modifier.worldOffset = tentacleTip.position - victim.agentPosition;
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPlayAnimation()
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			Debug.Log("HELLO" + (object)victim);
			_modifier = victim.Visual.GetNewTransformModifier();
			for (float t = 0f; t < duration; t += Time.deltaTime)
			{
				if (_modifier == null)
				{
					yield break;
				}
				tentacleTransform.localPosition = new Vector3(0f, yOffsetCurve.Evaluate(t / duration) - 5f, 0f);
				yield return null;
			}
			if (!_didDisableRenderer)
			{
				_didDisableRenderer = true;
				victim.Visual.DisableRenderersLocal();
			}
		}
	}

	protected static void InvokeUserCode_RpcPlayAnimation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayAnimation called on server.");
		}
		else
		{
			((Se_ObliviaxKidnap)(object)obj).UserCode_RpcPlayAnimation();
		}
	}

	static Se_ObliviaxKidnap()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Se_ObliviaxKidnap), "System.Void Se_ObliviaxKidnap::RpcPlayAnimation()", (RemoteCallDelegate)InvokeUserCode_RpcPlayAnimation);
	}
}
