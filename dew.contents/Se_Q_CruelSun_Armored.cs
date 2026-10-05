using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Se_Q_CruelSun_Armored : StatusEffect
{
	public ScalingValue armorAmount;

	[NonSerialized]
	[SyncVar]
	public bool doColdDamage;

	[NonSerialized]
	public bool disableArmor;

	private ScalingValue _baseArmorAmount;

	private DewEffect.FxColorSnapshot _baseColors;

	private bool _colorsAreCold;

	public override bool reuseInRoom => true;

	public bool NetworkdoColdDamage
	{
		get
		{
			return doColdDamage;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref doColdDamage, 4096uL, (Action<bool, bool>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_baseArmorAmount = armorAmount;
		_baseColors = DewEffect.CaptureColorsRecursively(((Component)(object)this).gameObject);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		armorAmount = _baseArmorAmount;
		if (_colorsAreCold)
		{
			DewEffect.RestoreColorsRecursively(_baseColors);
			_colorsAreCold = false;
		}
	}

	protected override void OnCreate()
	{
		if (doColdDamage)
		{
			DewEffect.ChangeColorRecursively(((Component)(object)this).gameObject, 0.55f, 0.7f, 0.7f);
			_colorsAreCold = true;
		}
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoUnstoppable();
			if (!disableArmor)
			{
				DoArmorBoost(GetValue(armorAmount));
			}
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, doColdDamage);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, doColdDamage);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref doColdDamage, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref doColdDamage, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
