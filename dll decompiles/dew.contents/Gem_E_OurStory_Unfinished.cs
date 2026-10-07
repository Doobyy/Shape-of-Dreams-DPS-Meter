using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Gem_E_OurStory_Unfinished : Gem
{
	public int killCount = 50;

	public int addedQuality = 50;

	public int completeThreshold = 500;

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	[SyncVar]
	public float statPercentage;

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	[SyncVar]
	public float dodgeEffectAmp;

	[SaveVar(SaveVarFlags.Default)]
	private int _currentKills;

	public float NetworkstatPercentage
	{
		get
		{
			return statPercentage;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref statPercentage, 262144uL, (Action<float, float>)null);
		}
	}

	public float NetworkdodgeEffectAmp
	{
		get
		{
			return dodgeEffectAmp;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref dodgeEffectAmp, 524288uL, (Action<float, float>)null);
		}
	}

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.ClientHeroEvent_OnKillOrAssist += new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
		}
	}

	private void ClientHeroEventOnKillOrAssist(EventInfoKill obj)
	{
		if (obj.victim is Monster)
		{
			_currentKills++;
			if (_currentKills >= killCount)
			{
				_currentKills -= killCount;
				quality += addedQuality;
			}
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)oldOwner != null)
		{
			oldOwner.ClientHeroEvent_OnKillOrAssist -= new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
		}
	}

	protected override void OnQualityChange(int oldQuality, int newQuality)
	{
		base.OnQualityChange(oldQuality, newQuality);
		if (((NetworkBehaviour)this).isServer && newQuality >= completeThreshold)
		{
			GemLocation loc = location;
			Hero hero = owner;
			Destroy();
			Gem_E_OurStory_Completed gem = Dew.CreateGem(hero.position, newQuality, null, (Gem_E_OurStory_Completed g) =>
			{
				g.NetworkstatPercentage = statPercentage;
				g.NetworkdodgeEffectAmp = dodgeEffectAmp;
			});
			hero.Skill.EquipGem(loc, gem);
			hero.Skill.RequestOnlyGemNotification(gem);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !owner.IsNullInactiveDeadOrKnockedOut() && !(owner is Hero_Bismuth) && UnityEngine.Random.value < 0.1f)
		{
			PureDamage(owner.maxHealth * 0.015f).SetAttr(DamageAttribute.DamageOverTime).Dispatch(owner);
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
			NetworkWriterExtensions.WriteFloat(writer, statPercentage);
			NetworkWriterExtensions.WriteFloat(writer, dodgeEffectAmp);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, statPercentage);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, dodgeEffectAmp);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref statPercentage, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref dodgeEffectAmp, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref statPercentage, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x80000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref dodgeEffectAmp, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
