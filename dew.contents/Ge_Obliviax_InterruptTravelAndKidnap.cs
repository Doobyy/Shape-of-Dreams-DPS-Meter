using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ge_Obliviax_InterruptTravelAndKidnap : GameEffect
{
	public GameObject fxStart;

	public GameObject fxBeforeKidnapEffect;

	public GameObject fxKidnapEffect;

	private bool _hasStartedKidnapSequence;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (isNewInstance && ManagerBase<CameraManager>.softInstance != null && !ManagerBase<CameraManager>.softInstance.focusedEntity.IsNullOrInactive())
		{
			DewEffect.Play(fxStart, ManagerBase<CameraManager>.softInstance.focusedEntity.agentPosition, null);
		}
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.AddTravelToNodeInterrupt(TravelInterrupt);
			if (isNewInstance)
			{
				NetworkedManagerBase<ChatManager>.instance.BroadcastMessage(new ChatManager.Message
				{
					type = ChatManager.MessageType.Notice,
					content = "Chat_Notice_ObliviaxWarning"
				});
			}
		}
	}

	private bool TravelInterrupt(EventInfoTravelToNodeInterrupt arg)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		return true;
		IEnumerator Routine()
		{
			if (!_hasStartedKidnapSequence)
			{
				_hasStartedKidnapSequence = true;
				RpcPlayBeforeKidnapEffect();
				foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
				{
					if (!gamePlayer.hero.IsNullInactiveDeadOrKnockedOut())
					{
						Hero target = gamePlayer.hero;
						CreateBasicEffect(target, new UncollidableEffect(), 5f);
						CreateBasicEffect(target, new DeathInterruptEffect
						{
							onInterrupt = (EventInfoKill _) =>
							{
								target.Status.SetHealth(1f);
							},
							priority = -9999
						}, 5f);
					}
				}
				yield return new WaitForSeconds(1.5f);
				bool flag = true;
				foreach (DewPlayer gamePlayer2 in DewPlayer.gamePlayers)
				{
					if (!gamePlayer2.hero.IsNullInactiveDeadOrKnockedOut())
					{
						CreateStatusEffect<Se_ObliviaxKidnap>(gamePlayer2.hero, default);
						flag = false;
					}
				}
				if (flag)
				{
					Destroy();
					FxStopNetworked(fxBeforeKidnapEffect);
					FxStopNetworked(fxKidnapEffect);
				}
				else
				{
					RpcPlayKidnapEffect();
					yield return new WaitForSeconds(3.6f);
					if (!NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
					{
						NetworkedManagerBase<ZoneManager>.instance.LoadSidetrackRoom("Room_Special_Obliviax_Nest");
					}
					Destroy();
					yield return new WaitForSeconds(1f);
					FxStopNetworked(fxBeforeKidnapEffect);
					FxStopNetworked(fxKidnapEffect);
				}
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.RemoveTravelToNodeInterrupt(TravelInterrupt);
		}
	}

	[ClientRpc]
	private void RpcPlayBeforeKidnapEffect()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ge_Obliviax_InterruptTravelAndKidnap::RpcPlayBeforeKidnapEffect()", 1211502984, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcPlayKidnapEffect()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ge_Obliviax_InterruptTravelAndKidnap::RpcPlayKidnapEffect()", 754329641, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPlayBeforeKidnapEffect()
	{
		DewEffect.Play(fxBeforeKidnapEffect, ManagerBase<CameraManager>.instance.focusedEntity.agentPosition, null);
	}

	protected static void InvokeUserCode_RpcPlayBeforeKidnapEffect(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayBeforeKidnapEffect called on server.");
		}
		else
		{
			((Ge_Obliviax_InterruptTravelAndKidnap)(object)obj).UserCode_RpcPlayBeforeKidnapEffect();
		}
	}

	protected void UserCode_RpcPlayKidnapEffect()
	{
		DewEffect.Play(fxKidnapEffect, ManagerBase<CameraManager>.instance.focusedEntity.agentPosition, null);
	}

	protected static void InvokeUserCode_RpcPlayKidnapEffect(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayKidnapEffect called on server.");
		}
		else
		{
			((Ge_Obliviax_InterruptTravelAndKidnap)(object)obj).UserCode_RpcPlayKidnapEffect();
		}
	}

	static Ge_Obliviax_InterruptTravelAndKidnap()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ge_Obliviax_InterruptTravelAndKidnap), "System.Void Ge_Obliviax_InterruptTravelAndKidnap::RpcPlayBeforeKidnapEffect()", (RemoteCallDelegate)InvokeUserCode_RpcPlayBeforeKidnapEffect);
		RemoteProcedureCalls.RegisterRpc(typeof(Ge_Obliviax_InterruptTravelAndKidnap), "System.Void Ge_Obliviax_InterruptTravelAndKidnap::RpcPlayKidnapEffect()", (RemoteCallDelegate)InvokeUserCode_RpcPlayKidnapEffect);
	}
}
