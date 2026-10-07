using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class St_E_Rewind : SkillTrigger
{
	[SyncVar]
	private HeroSkillLocation _targetType;

	[SyncVar]
	private bool _isSet;

	public HeroSkillLocation Network_targetType
	{
		get
		{
			return _targetType;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<HeroSkillLocation>(value, ref _targetType, 134217728uL, (Action<HeroSkillLocation, HeroSkillLocation>)null);
		}
	}

	public bool Network_isSet
	{
		get
		{
			return _isSet;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isSet, 268435456uL, (Action<bool, bool>)null);
		}
	}

	public override void OnCastCompleteBeforePrepare(EventInfoCast cast)
	{
		base.OnCastCompleteBeforePrepare(cast);
		if (cast.instance is Se_E_Rewind se_E_Rewind)
		{
			owner.Skill.TryGetSkill(_targetType, out se_E_Rewind.targetSkill);
		}
	}

	protected override void OnEquip(Entity newOwner)
	{
		base.OnEquip(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			((Hero)newOwner).ClientHeroEvent_OnSkillUse += new Action<EventInfoSkillUse>(HeroEventOnSkillUse);
			Network_isSet = false;
		}
	}

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			((Hero)formerOwner).ClientHeroEvent_OnSkillUse -= new Action<EventInfoSkillUse>(HeroEventOnSkillUse);
		}
	}

	private void HeroEventOnSkillUse(EventInfoSkillUse obj)
	{
		if ((obj.type == HeroSkillLocation.Q || obj.type == HeroSkillLocation.W || obj.type == HeroSkillLocation.E || obj.type == HeroSkillLocation.R) && !((UnityEngine.Object)(object)obj.skill == (UnityEngine.Object)(object)this))
		{
			Network_isSet = true;
			Network_targetType = obj.type;
		}
	}

	public override bool CanBeCast()
	{
		if (!base.CanBeCast())
		{
			return false;
		}
		if (_isSet && owner.Skill.TryGetSkill(_targetType, out var _))
		{
			if (owner.Status.TryGetStatusEffect<Se_E_Rewind>(out var effect))
			{
				return effect._didConsume;
			}
			return true;
		}
		return false;
	}

	public override bool CanBeReserved()
	{
		if (!base.CanBeReserved())
		{
			return false;
		}
		if (_isSet && owner.Skill.TryGetSkill(_targetType, out var _))
		{
			if (owner.Status.TryGetStatusEffect<Se_E_Rewind>(out var effect))
			{
				return effect._didConsume;
			}
			return true;
		}
		return false;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_HeroSkillLocation(writer, _targetType);
			NetworkWriterExtensions.WriteBool(writer, _isSet);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x8000000L) != 0L)
		{
			GeneratedNetworkCode._Write_HeroSkillLocation(writer, _targetType);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10000000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isSet);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<HeroSkillLocation>(ref _targetType, (Action<HeroSkillLocation, HeroSkillLocation>)null, GeneratedNetworkCode._Read_HeroSkillLocation(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isSet, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x8000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<HeroSkillLocation>(ref _targetType, (Action<HeroSkillLocation, HeroSkillLocation>)null, GeneratedNetworkCode._Read_HeroSkillLocation(reader));
		}
		if ((num & 0x10000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isSet, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
