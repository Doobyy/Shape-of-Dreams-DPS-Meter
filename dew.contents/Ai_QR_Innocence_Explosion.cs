using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_QR_Innocence_Explosion : InstantDamageInstance
{
	public float slowDuration = 0.5f;

	public float slowAmount = 50f;

	[NonSerialized]
	[SyncVar]
	public ElementalType elementalOverride;

	private DewEffect.FxColorSnapshot _colorSnapshot;

	private ElementalType _appliedElemental;

	private bool _hasAppliedElemental;

	public override bool reuseInRoom => true;

	public ElementalType NetworkelementalOverride
	{
		get
		{
			return elementalOverride;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<ElementalType>(value, ref elementalOverride, 128uL, (Action<ElementalType, ElementalType>)null);
		}
	}

	protected override void OnCreate()
	{
		elemental = elementalOverride;
		if (_colorSnapshot == null)
		{
			_colorSnapshot = DewEffect.CaptureColorsRecursively(((Component)(object)this).gameObject);
			ApplyElementalColors();
		}
		else if (!_hasAppliedElemental || _appliedElemental != elemental)
		{
			DewEffect.RestoreColorsRecursively(_colorSnapshot);
			ApplyElementalColors();
		}
		base.OnCreate();
	}

	private void ApplyElementalColors()
	{
		_appliedElemental = elemental;
		_hasAppliedElemental = true;
		if (elemental == ElementalType.Fire)
		{
			DewEffect.ChangeColorRecursively(((Component)(object)this).gameObject, 0.08333f);
		}
		else if (elemental == ElementalType.Cold)
		{
			DewEffect.ChangeColorRecursively(((Component)(object)this).gameObject, 0.49166f, 0.25f);
		}
		else if (elemental == ElementalType.Dark)
		{
			DewEffect.ChangeColorRecursively(((Component)(object)this).gameObject, 0.77222f);
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		if (slowDuration > 0f && slowAmount > 0f)
		{
			CreateBasicEffect(entity, new SlowEffect
			{
				strength = slowAmount
			}, slowDuration);
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
			GeneratedNetworkCode._Write_ElementalType(writer, elementalOverride);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			GeneratedNetworkCode._Write_ElementalType(writer, elementalOverride);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<ElementalType>(ref elementalOverride, (Action<ElementalType, ElementalType>)null, GeneratedNetworkCode._Read_ElementalType(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<ElementalType>(ref elementalOverride, (Action<ElementalType, ElementalType>)null, GeneratedNetworkCode._Read_ElementalType(reader));
		}
	}
}
