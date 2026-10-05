using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class SpawnGemStarProp : DewNetworkBehaviour, IInteractable, IActivatable
{
	public float effectDelay;

	public DewCollider range;

	public AbilityTargetValidator hittable;

	public Knockback knockback;

	[SyncVar]
	public bool isActivated;

	public GameObject fxActivate;

	private Room_Barrier _obstacle;

	int IInteractable.priority => 50;

	public bool canInteractWithMouse => false;

	public Transform interactPivot => ((Component)(object)this).transform;

	public float focusDistance => 3f;

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
		_obstacle = ((Component)(object)this).GetComponent<Room_Barrier>();
	}

	public bool CanInteract(Entity entity)
	{
		return !isActivated;
	}

	public void OnInteract(Entity entity, bool alt)
	{
		FxPlayNew(fxActivate);
		Vector3 pos;
		int quality;
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkisActivated = true;
			pos = Dew.GetPositionOnGround(((Component)(object)this).transform.position);
			quality = NetworkedManagerBase<LootManager>.instance.SelectGemQuality(Rarity.Rare) * 2;
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			List<Entity> entities = range.GetEntities(out var handle, hittable, entity);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity to = entities[i];
				knockback.ApplyWithOrigin(pos, to);
			}
			handle.Return();
			yield return new SI.WaitForSeconds(effectDelay);
			Dew.CreateGem<Gem_R_Celestial>(pos, quality, entity.owner);
			UnityEngine.Object.Destroy(((Component)(object)this).gameObject);
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
