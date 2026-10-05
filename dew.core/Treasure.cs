using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

[DewResourceLink(ResourceLinkBy.Type)]
public abstract class Treasure : Actor, IExcludeFromPool
{
	public Sprite icon;

	public int maxUse = 1;

	public int basePrice = 100;

	public float chanceWeight = 1f;

	public bool excludeFromPool;

	[CompilerGenerated]
	[SyncVar]
	private int price__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private PropEnt_Merchant_Base merchant__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private DewPlayer player__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private Hero hero__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private string customData__BackingField;

	protected NetworkBehaviourSyncVar ____003Cmerchant_003Ek__BackingFieldNetId;

	protected NetworkBehaviourSyncVar ____003Cplayer_003Ek__BackingFieldNetId;

	protected NetworkBehaviourSyncVar ____003Chero_003Ek__BackingFieldNetId;

	public int price
	{
		[CompilerGenerated]
		get
		{
			return price__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cprice_003Ek__BackingField = value;
		}
	}

	public PropEnt_Merchant_Base merchant
	{
		[CompilerGenerated]
		get
		{
			return Network_003Cmerchant_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cmerchant_003Ek__BackingField = value;
		}
	}

	public DewPlayer player
	{
		[CompilerGenerated]
		get
		{
			return Network_003Cplayer_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cplayer_003Ek__BackingField = value;
		}
	}

	public Hero hero
	{
		[CompilerGenerated]
		get
		{
			return Network_003Chero_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Chero_003Ek__BackingField = value;
		}
	}

	public string customData
	{
		[CompilerGenerated]
		get
		{
			return customData__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CcustomData_003Ek__BackingField = value;
		}
	}

	bool IExcludeFromPool.excludeFromPool => excludeFromPool;

	public int Network_003Cprice_003Ek__BackingField
	{
		get
		{
			return price__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref price__BackingField, 8uL, (Action<int, int>)null);
		}
	}

	public PropEnt_Merchant_Base Network_003Cmerchant_003Ek__BackingField
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<PropEnt_Merchant_Base>(____003Cmerchant_003Ek__BackingFieldNetId, ref merchant__BackingField);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<PropEnt_Merchant_Base>(value, ref merchant__BackingField, 16uL, (Action<PropEnt_Merchant_Base, PropEnt_Merchant_Base>)null, ref ____003Cmerchant_003Ek__BackingFieldNetId);
		}
	}

	public DewPlayer Network_003Cplayer_003Ek__BackingField
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<DewPlayer>(____003Cplayer_003Ek__BackingFieldNetId, ref player__BackingField);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<DewPlayer>(value, ref player__BackingField, 32uL, (Action<DewPlayer, DewPlayer>)null, ref ____003Cplayer_003Ek__BackingFieldNetId);
		}
	}

	public Hero Network_003Chero_003Ek__BackingField
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<Hero>(____003Chero_003Ek__BackingFieldNetId, ref hero__BackingField);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<Hero>(value, ref hero__BackingField, 64uL, (Action<Hero, Hero>)null, ref ____003Chero_003Ek__BackingFieldNetId);
		}
	}

	public string Network_003CcustomData_003Ek__BackingField
	{
		get
		{
			return customData__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref customData__BackingField, 128uL, (Action<string, string>)null);
		}
	}

	public virtual void OnAddMerchandise(out Cost mercPrice, out string customData)
	{
		float adjustedGoldAmount_Cost_Service = NetworkedManagerBase<GameManager>.instance.GetAdjustedGoldAmount_Cost_Service(basePrice);
		mercPrice = Cost.Gold(Mathf.RoundToInt(adjustedGoldAmount_Cost_Service));
		customData = null;
	}

	public virtual bool ShouldBeIncludedInPool()
	{
		return true;
	}

	public virtual bool CanBePurchased()
	{
		return true;
	}

	public virtual string GetCustomName()
	{
		return null;
	}

	public virtual string GetCustomDescription()
	{
		return null;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteInt(writer, price__BackingField);
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003Cmerchant_003Ek__BackingField);
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003Cplayer_003Ek__BackingField);
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003Chero_003Ek__BackingField);
			NetworkWriterExtensions.WriteString(writer, customData__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, price__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003Cmerchant_003Ek__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003Cplayer_003Ek__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003Chero_003Ek__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, customData__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref price__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<PropEnt_Merchant_Base>(ref merchant__BackingField, (Action<PropEnt_Merchant_Base, PropEnt_Merchant_Base>)null, reader, ref ____003Cmerchant_003Ek__BackingFieldNetId);
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<DewPlayer>(ref player__BackingField, (Action<DewPlayer, DewPlayer>)null, reader, ref ____003Cplayer_003Ek__BackingFieldNetId);
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Hero>(ref hero__BackingField, (Action<Hero, Hero>)null, reader, ref ____003Chero_003Ek__BackingFieldNetId);
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref customData__BackingField, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref price__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x10L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<PropEnt_Merchant_Base>(ref merchant__BackingField, (Action<PropEnt_Merchant_Base, PropEnt_Merchant_Base>)null, reader, ref ____003Cmerchant_003Ek__BackingFieldNetId);
		}
		if ((num & 0x20L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<DewPlayer>(ref player__BackingField, (Action<DewPlayer, DewPlayer>)null, reader, ref ____003Cplayer_003Ek__BackingFieldNetId);
		}
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Hero>(ref hero__BackingField, (Action<Hero, Hero>)null, reader, ref ____003Chero_003Ek__BackingFieldNetId);
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref customData__BackingField, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
		}
	}
}
