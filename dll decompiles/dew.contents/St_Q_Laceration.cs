using System;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class St_Q_Laceration : SkillTrigger
{
	public Sprite altSprite;

	public CastMethodData altMethod;

	public GameObject fxPrimary;

	public GameObject fxAlt;

	[NonSerialized]
	public bool lockBlueForm;

	private Sprite _ogSprite;

	private AssetRef<AbilityInstance> _ogInstance;

	private CastMethodData _ogMethod;

	private bool _isAlt => configs[0].triggerIcon != _ogSprite;

	[Server]
	public void SetLockBlueForm(bool locked)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void St_Q_Laceration::SetLockBlueForm(System.Boolean)' called when server was not active");
			return;
		}
		lockBlueForm = locked;
		if (locked && !_isAlt)
		{
			RpcSwitchMode(alt: true);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		_ogSprite = configs[0].triggerIcon;
		_ogInstance = configs[0].spawnedInstance;
		_ogMethod = configs[0].castMethod;
	}

	protected override void OnEquip(Entity newOwner)
	{
		base.OnEquip(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.EntityEvent_OnCastStart += new Action<EventInfoCast>(OnCastStart);
			FxPlayNetworked(_isAlt ? fxAlt : fxPrimary, newOwner);
		}
	}

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)formerOwner != null)
			{
				formerOwner.EntityEvent_OnCastStart -= new Action<EventInfoCast>(OnCastStart);
			}
			FxStopNetworked(fxPrimary);
			FxStopNetworked(fxAlt);
		}
	}

	private void OnCastStart(EventInfoCast obj)
	{
		if (obj.trigger is AttackTrigger && !lockBlueForm)
		{
			RpcSwitchMode(!_isAlt);
		}
	}

	[ClientRpc]
	private void RpcSwitchMode(bool alt)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, alt);
		((NetworkBehaviour)this).SendRPCInternal("System.Void St_Q_Laceration::RpcSwitchMode(System.Boolean)", -1671188590, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcSwitchMode__Boolean(bool alt)
	{
		FxPlay(alt ? fxAlt : fxPrimary, owner);
		FxStop((!alt) ? fxAlt : fxPrimary);
		if (alt)
		{
			configs[0].triggerIcon = altSprite;
			configs[0].spawnedInstance = DewResources.GetByType<Ai_Q_Laceration_Circle>(default(ResourceLoadSettings));
			configs[0].castMethod = altMethod;
		}
		else
		{
			configs[0].triggerIcon = _ogSprite;
			configs[0].spawnedInstance = _ogInstance;
			configs[0].castMethod = _ogMethod;
		}
	}

	protected static void InvokeUserCode_RpcSwitchMode__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSwitchMode called on server.");
		}
		else
		{
			((St_Q_Laceration)(object)obj).UserCode_RpcSwitchMode__Boolean(NetworkReaderExtensions.ReadBool(reader));
		}
	}

	static St_Q_Laceration()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(St_Q_Laceration), "System.Void St_Q_Laceration::RpcSwitchMode(System.Boolean)", (RemoteCallDelegate)InvokeUserCode_RpcSwitchMode__Boolean);
	}
}
