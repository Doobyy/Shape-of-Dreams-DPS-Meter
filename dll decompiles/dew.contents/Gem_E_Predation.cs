using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Gem_E_Predation : Gem
{
	public ScalingValue amp;

	public float killGraceTime;

	public int bossMultiplier = 3;

	[NonSerialized]
	[SyncVar]
	public float boostedAp;

	[NonSerialized]
	[SyncVar]
	public float boostedAd;

	[NonSerialized]
	[SyncVar]
	public float boostedHealth;

	private KillTracker _tracker;

	public float NetworkboostedAp
	{
		get
		{
			return boostedAp;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref boostedAp, 262144uL, (Action<float, float>)null);
		}
	}

	public float NetworkboostedAd
	{
		get
		{
			return boostedAd;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref boostedAd, 524288uL, (Action<float, float>)null);
		}
	}

	public float NetworkboostedHealth
	{
		get
		{
			return boostedHealth;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref boostedHealth, 1048576uL, (Action<float, float>)null);
		}
	}

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			_tracker = newOwner.TrackKills(killGraceTime, OnKill);
			newOwner.ClientEntityEvent_OnStatusEffectAdded += new Action<EventInfoStatusEffect>(ClientEntityEventOnStatusEffectAdded);
			UpdateStack();
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			if ((bool)(UnityEngine.Object)(object)oldOwner)
			{
				oldOwner.ClientEntityEvent_OnStatusEffectAdded -= new Action<EventInfoStatusEffect>(ClientEntityEventOnStatusEffectAdded);
			}
			if (_tracker != null)
			{
				_tracker.Stop();
				_tracker = null;
			}
			UpdateStack();
		}
	}

	private void OnKill(EventInfoKill obj)
	{
		if (!(obj.victim is Monster monster) || !isValid || owner.IsNullInactiveDeadOrKnockedOut())
		{
			return;
		}
		if (monster.IsAnyBoss())
		{
			for (int i = 0; i < bossMultiplier; i++)
			{
				CreateAbilityInstance(monster.position, null, new CastInfo(owner), (Ai_Gem_E_Predation_Pickup p) =>
				{
					p._targetHero = owner;
				});
			}
			NotifyUse();
		}
		else if (monster.isHunter)
		{
			CreateAbilityInstance(monster.position, null, new CastInfo(owner), (Ai_Gem_E_Predation_Pickup p) =>
			{
				p._targetHero = owner;
			});
			NotifyUse();
		}
	}

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			newSkill.dealtDamageProcessor.Add(GiveAmpDmg);
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)oldSkill != null)
		{
			oldSkill.dealtDamageProcessor.Remove(GiveAmpDmg);
		}
	}

	private void GiveAmpDmg(ref DamageData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this) && target is Monster monster && (monster.type == Monster.MonsterType.Boss || monster.isHunter || monster.type == Monster.MonsterType.MiniBoss))
		{
			data.SetAttr(DamageAttribute.IsCrit);
			data.ApplyAmplification(GetValue(amp));
			data.SetAmountModifiedBy(this);
			NotifyUse();
		}
	}

	[Server]
	public void UpdateStack()
	{
		Se_Gem_E_Predation_StatBonus effect;
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Gem_E_Predation::UpdateStack()' called when server was not active");
		}
		else if ((UnityEngine.Object)(object)owner == null || !owner.Status.TryGetStatusEffect<Se_Gem_E_Predation_StatBonus>(out effect))
		{
			NetworkboostedAd = 0f;
			NetworkboostedAp = 0f;
			NetworkboostedHealth = 0f;
		}
		else
		{
			NetworkboostedAd = effect.bonus.attackDamageFlat;
			NetworkboostedAp = effect.bonus.abilityPowerFlat;
			NetworkboostedHealth = effect.bonus.maxHealthFlat;
		}
	}

	private void ClientEntityEventOnStatusEffectAdded(EventInfoStatusEffect obj)
	{
		if (obj.effect is Se_Gem_E_Predation_StatBonus)
		{
			UpdateStack();
			owner.ClientEntityEvent_OnStatusEffectAdded -= new Action<EventInfoStatusEffect>(ClientEntityEventOnStatusEffectAdded);
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
			NetworkWriterExtensions.WriteFloat(writer, boostedAp);
			NetworkWriterExtensions.WriteFloat(writer, boostedAd);
			NetworkWriterExtensions.WriteFloat(writer, boostedHealth);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, boostedAp);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, boostedAd);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, boostedHealth);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref boostedAp, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref boostedAd, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref boostedHealth, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref boostedAp, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x80000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref boostedAd, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x100000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref boostedHealth, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
