using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_QR_Innocence_Projectile : StandardProjectile
{
	[NonSerialized]
	[SyncVar]
	public ElementalType elemental = ElementalType.Light;

	[NonSerialized]
	public float strengthMultiplier = 1f;

	private DewEffect.FxColorSnapshot _colorSnapshot;

	private ElementalType _appliedElemental;

	public override bool reuseInRoom => true;

	public ElementalType Networkelemental
	{
		get
		{
			return elemental;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<ElementalType>(value, ref elemental, 524288uL, (Action<ElementalType, ElementalType>)null);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Networkelemental = ElementalType.Light;
		strengthMultiplier = 1f;
	}

	protected override void OnCreate()
	{
		if (_colorSnapshot == null)
		{
			_colorSnapshot = DewEffect.CaptureColorsRecursively(((Component)(object)this).gameObject);
			ApplyElementalColors();
		}
		else if (_appliedElemental != elemental)
		{
			DewEffect.RestoreColorsRecursively(_colorSnapshot);
			ApplyElementalColors();
		}
		base.OnCreate();
	}

	private void ApplyElementalColors()
	{
		_appliedElemental = elemental;
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

	protected override void OnComplete()
	{
		base.OnComplete();
		CreateAbilityInstance(Dew.GetPositionOnGround(position), null, new CastInfo(info.caster, position), (Ai_QR_Innocence_Explosion exp) =>
		{
			exp.NetworkelementalOverride = elemental;
			exp.strengthMultiplier = strengthMultiplier;
		});
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_ElementalType(writer, elemental);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80000L) != 0L)
		{
			GeneratedNetworkCode._Write_ElementalType(writer, elemental);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<ElementalType>(ref elemental, (Action<ElementalType, ElementalType>)null, GeneratedNetworkCode._Read_ElementalType(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<ElementalType>(ref elemental, (Action<ElementalType, ElementalType>)null, GeneratedNetworkCode._Read_ElementalType(reader));
		}
	}
}
