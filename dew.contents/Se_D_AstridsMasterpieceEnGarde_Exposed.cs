using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Se_D_AstridsMasterpieceEnGarde_Exposed : StatusEffect
{
	public int maxCharge = 3;

	public MeshRenderer chargeRenderer;

	public Material[] perChargeMaterials;

	[NonSerialized]
	[SyncVar]
	private int _chargeCount;

	[SyncVar]
	private float _chargeElapsedTime;

	private Action<bool> _cachedOnRendererEnabledChanged;

	public override bool reuseInRoom => true;

	public float rechargeTime => Mathf.Max(0.5f, 6f - 0.7f * (float)(skillLevel - 1));

	public int chargeCount
	{
		get
		{
			return _chargeCount;
		}
		set
		{
			Network_chargeCount = value;
		}
	}

	public int Network_chargeCount
	{
		get
		{
			return _chargeCount;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref _chargeCount, 4096uL, (Action<int, int>)null);
		}
	}

	public float Network_chargeElapsedTime
	{
		get
		{
			return _chargeElapsedTime;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _chargeElapsedTime, 8192uL, (Action<float, float>)null);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Network_chargeElapsedTime = 0f;
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		Network_chargeCount = maxCharge;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		UpdateVisual();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			DestroyOnDestroy(parentActor);
			victim.Visual.ClientEvent_OnRendererEnabledChanged += new Action<bool>(ClientEventOnRendererEnabledChanged);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.Visual.ClientEvent_OnRendererEnabledChanged -= _cachedOnRendererEnabledChanged;
		}
	}

	private void ClientEventOnRendererEnabledChanged(bool obj)
	{
		UpdateVisual();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		UpdateVisual();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (chargeCount <= 0)
		{
			Network_chargeElapsedTime = _chargeElapsedTime + dt;
			if (_chargeElapsedTime > rechargeTime)
			{
				Network_chargeElapsedTime = 0f;
				chargeCount = maxCharge;
			}
		}
		if ((UnityEngine.Object)(object)info.caster == null || !info.caster.isActive || !info.caster.CheckEnemyOrNeutral(victim))
		{
			Destroy();
		}
	}

	private void UpdateVisual()
	{
		if ((bool)chargeRenderer && (bool)(UnityEngine.Object)(object)victim)
		{
			int num = chargeCount - 1;
			if (num >= 0 && num < perChargeMaterials.Length && !victim.Visual.isRendererOff)
			{
				chargeRenderer.enabled = true;
				chargeRenderer.sharedMaterial = perChargeMaterials[chargeCount - 1];
			}
			else
			{
				chargeRenderer.enabled = false;
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
			NetworkWriterExtensions.WriteInt(writer, _chargeCount);
			NetworkWriterExtensions.WriteFloat(writer, _chargeElapsedTime);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, _chargeCount);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x2000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _chargeElapsedTime);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _chargeCount, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _chargeElapsedTime, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _chargeCount, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x2000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _chargeElapsedTime, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
