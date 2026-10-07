using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Room_Shortcut : DewNetworkBehaviour, IInteractable, ICustomInteractable, IPlayerPathablePoint
{
	public Room_Shortcut targetShortcut;

	public Transform walkStartPos;

	public Transform walkEndPos;

	public GameObject openEffect;

	public GameObject closedEffect;

	[SyncVar(hook = "OnIsOpenChanged")]
	public bool isOpen;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate_isOpen;

	Transform IInteractable.interactPivot => ((Component)(object)this).transform;

	bool IInteractable.canInteractWithMouse => false;

	float IInteractable.focusDistance => 2.5f;

	int IInteractable.priority => 101;

	string ICustomInteractable.nameRawText => DewLocalization.GetUIValue("InGame_Interact_Shortcut");

	string ICustomInteractable.interactActionRawText => DewLocalization.GetUIValue("InGame_Interact_Shortcut_Enter");

	string ICustomInteractable.interactAltActionRawText => null;

	public float? altInteractProgress => null;

	public Cost cost => default;

	bool ICustomInteractable.canAltInteract => false;

	Vector3 IPlayerPathablePoint.pathablePosition => Dew.GetPositionOnGround(walkStartPos.position);

	public bool NetworkisOpen
	{
		get
		{
			return isOpen;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isOpen, 1uL, _Mirror_SyncVarHookDelegate_isOpen);
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		OnIsOpenChanged(oldValue: false, isOpen);
	}

	private void OnIsOpenChanged(bool oldValue, bool newValue)
	{
		if (newValue)
		{
			if (openEffect != null)
			{
				FxPlay(openEffect);
			}
			if (closedEffect != null)
			{
				FxStop(closedEffect);
			}
		}
		else
		{
			if (openEffect != null)
			{
				FxStop(openEffect);
			}
			if (closedEffect != null)
			{
				FxPlay(closedEffect);
			}
		}
	}

	[Server]
	public void Open()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room_Shortcut::Open()' called when server was not active");
		}
		else
		{
			NetworkisOpen = true;
		}
	}

	[Server]
	public void Close()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room_Shortcut::Close()' called when server was not active");
		}
		else
		{
			NetworkisOpen = false;
		}
	}

	private void OnValidate()
	{
		if ((UnityEngine.Object)(object)targetShortcut != null)
		{
			targetShortcut.targetShortcut = this;
		}
	}

	private void OnDrawGizmos()
	{
		if (walkStartPos != null && walkEndPos != null)
		{
			DewGizmos.DrawArrow(walkStartPos.position, walkEndPos.position, Color.magenta, 1f);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if ((UnityEngine.Object)(object)targetShortcut != null)
		{
			DewGizmos.DrawLine(((Component)(object)this).transform.position, ((Component)(object)targetShortcut).transform.position, Color.magenta);
		}
	}

	bool IInteractable.CanInteract(Entity entity)
	{
		return true;
	}

	void IInteractable.OnInteract(Entity entity, bool alt)
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (entity is Hero { isInCombat: not false } hero)
		{
			hero.owner.TpcShowCenterMessage(CenterMessageType.Error, "InGame_Message_ShortcutUnavailableInCombat");
		}
		else if (!entity.Status.HasStatusEffect<Se_UsingShortcut>())
		{
			entity.CreateStatusEffect(entity, default, (Se_UsingShortcut se) =>
			{
				se.startShortcut = this;
				se.targetShortcut = targetShortcut;
			});
		}
	}

	public Room_Shortcut()
	{
		_Mirror_SyncVarHookDelegate_isOpen = OnIsOpenChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, isOpen);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isOpen);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isOpen, _Mirror_SyncVarHookDelegate_isOpen, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isOpen, _Mirror_SyncVarHookDelegate_isOpen, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
