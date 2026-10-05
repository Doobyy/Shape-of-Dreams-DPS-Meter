using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_BossNyx_Swipe : InstantDamageInstance
{
	public DewCollider improvedRange;

	public GameObject improvedStartEffect;

	[NonSerialized]
	[SyncVar]
	public bool enableImprovedSwipe;

	private DewCollider _pristineRange;

	private GameObject _pristineStartEffect;

	public override bool reuseInRoom => true;

	public bool NetworkenableImprovedSwipe
	{
		get
		{
			return enableImprovedSwipe;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref enableImprovedSwipe, 128uL, (Action<bool, bool>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_pristineRange = range;
		_pristineStartEffect = startEffect;
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		if (enableImprovedSwipe)
		{
			range = improvedRange;
		}
	}

	protected override void OnCreate()
	{
		if (enableImprovedSwipe)
		{
			startEffect = improvedStartEffect;
		}
		if (SingletonBehaviour<Sky_BossRoomCenter>.instance != null)
		{
			float num = Dew.GetPositionOnGround(SingletonBehaviour<Sky_BossRoomCenter>.instance.transform.position).y + 0.15f;
			if (((Component)(object)this).transform.position.y < num)
			{
				((Component)(object)this).transform.position = ((Component)(object)this).transform.position.WithY(num);
			}
		}
		base.OnCreate();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		range = _pristineRange;
		startEffect = _pristineStartEffect;
		NetworkenableImprovedSwipe = false;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, enableImprovedSwipe);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, enableImprovedSwipe);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref enableImprovedSwipe, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref enableImprovedSwipe, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
