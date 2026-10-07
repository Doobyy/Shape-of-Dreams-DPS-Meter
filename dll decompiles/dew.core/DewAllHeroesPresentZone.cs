using System;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(DewCollider))]
public class DewAllHeroesPresentZone : DewNetworkBehaviour
{
	public bool once = true;

	public UnityEvent onActivate;

	private List<Hero> _currentMembers = new List<Hero>();

	private float _lastCheckTime;

	private int _lastNumOfHeroes;

	private DewCollider _collider;

	public IReadOnlyList<Hero> currentMembers => _currentMembers;

	protected override void Awake()
	{
		base.Awake();
		_collider = ((Component)(object)this).GetComponent<DewCollider>();
	}

	private void Update()
	{
		if (!((NetworkBehaviour)this).isServer || !(Time.time - _lastCheckTime > 1f))
		{
			return;
		}
		_lastCheckTime = Time.time;
		int num = 0;
		int num2 = 0;
		_currentMembers.Clear();
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			if (!allHero.isKnockedOut)
			{
				num++;
				if (_collider.OverlapPoint(allHero.position.ToXY()))
				{
					num2++;
					_currentMembers.Add(allHero);
				}
			}
		}
		if (num2 > 0 && num2 != _lastNumOfHeroes && num2 < num)
		{
			RpcShowNeedPresenceMessage(num2, num);
		}
		_lastNumOfHeroes = num2;
		if (num > 0 && num <= num2)
		{
			try
			{
				onActivate?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			if (once)
			{
				((Behaviour)(object)this).enabled = false;
			}
		}
	}

	[ClientRpc]
	private void RpcShowNeedPresenceMessage(int curr, int max)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, curr);
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, max);
		((NetworkBehaviour)this).SendRPCInternal("System.Void DewAllHeroesPresentZone::RpcShowNeedPresenceMessage(System.Int32,System.Int32)", -35612767, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcShowNeedPresenceMessage__Int32__Int32(int curr, int max)
	{
		InGameUIManager.instance.ShowCenterMessage(CenterMessageType.General, "InGame_Message_NeedAllPlayersPresence", new object[2] { curr, max });
	}

	protected static void InvokeUserCode_RpcShowNeedPresenceMessage__Int32__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowNeedPresenceMessage called on server.");
		}
		else
		{
			((DewAllHeroesPresentZone)(object)obj).UserCode_RpcShowNeedPresenceMessage__Int32__Int32(NetworkReaderExtensions.ReadInt(reader), NetworkReaderExtensions.ReadInt(reader));
		}
	}

	static DewAllHeroesPresentZone()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(DewAllHeroesPresentZone), "System.Void DewAllHeroesPresentZone::RpcShowNeedPresenceMessage(System.Int32,System.Int32)", (RemoteCallDelegate)InvokeUserCode_RpcShowNeedPresenceMessage__Int32__Int32);
	}
}
