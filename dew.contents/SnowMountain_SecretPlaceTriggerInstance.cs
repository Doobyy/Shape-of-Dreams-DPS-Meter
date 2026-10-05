using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class SnowMountain_SecretPlaceTriggerInstance : DewNetworkBehaviour, IInteractable, IActivatable
{
	[SyncVar]
	public bool isActivated;

	public SnowMountain_OpenSecretPlace gate;

	public GameObject activateSound;

	public float intensityAmp;

	private Material _mat;

	private float _emissionIntensity;

	private Color _emissionColor;

	int IInteractable.priority => 50;

	public Transform interactPivot => ((Component)(object)this).transform;

	public bool canInteractWithMouse => false;

	public float focusDistance => 2.5f;

	public bool NetworkisActivated
	{
		get
		{
			return isActivated;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isActivated, 1uL, (Action<bool, bool>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_emissionColor = new Color(77f, 134f, 191f);
		_mat = ((Component)(object)this).GetComponent<MeshRenderer>().material;
		_mat.SetColor("_EmissionColor", _emissionColor * _emissionIntensity);
	}

	public bool CanInteract(Entity entity)
	{
		return !isActivated;
	}

	public void OnInteract(Entity entity, bool alt)
	{
		FxPlay(activateSound);
		((MonoBehaviour)(object)this).StartCoroutine(OnInteractRoutine());
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkisActivated = true;
			gate.OnTriggerActivated?.Invoke();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		UnityEngine.Object.Destroy(_mat);
	}

	private IEnumerator OnInteractRoutine()
	{
		while ((double)_emissionIntensity < 0.02)
		{
			_emissionIntensity += intensityAmp / 10000f;
			_mat.SetColor("_EmissionColor", _emissionColor * _emissionIntensity);
			yield return new WaitForSeconds(0.1f);
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, isActivated);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isActivated);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isActivated, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isActivated, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
