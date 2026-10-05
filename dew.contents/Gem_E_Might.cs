using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Gem_E_Might : Gem
{
	public ScalingValue healthBonus;

	public float updateInterval;

	public float unitHealthAmount = 50f;

	public ScalingValue dmgAmpPerUnitHealth;

	[NonSerialized]
	[SyncVar]
	private float _currentDamageAmp;

	private float _lastUpdateTime;

	public float currentDamageAmp
	{
		get
		{
			if ((!NetworkServer.active && !NetworkClient.active) || !((UnityEngine.Object)(object)((NetworkBehaviour)this).netIdentity != null) || !((UnityEngine.Object)(object)owner != null))
			{
				return GetDamageAmpFallback();
			}
			return _currentDamageAmp;
		}
	}

	public float Network_currentDamageAmp
	{
		get
		{
			return _currentDamageAmp;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _currentDamageAmp, 262144uL, (Action<float, float>)null);
		}
	}

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			newSkill.dealtDamageProcessor.Add(Amplify);
			statBonus.maxHealthFlat = GetValue(healthBonus);
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)oldSkill != null)
		{
			oldSkill.dealtDamageProcessor.Remove(Amplify);
		}
	}

	protected override void OnQualityChange(int oldQuality, int newQuality)
	{
		base.OnQualityChange(oldQuality, newQuality);
		if (((NetworkBehaviour)this).isServer)
		{
			statBonus.maxHealthFlat = GetValue(healthBonus);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && isValid && !(Time.time - _lastUpdateTime < updateInterval))
		{
			_lastUpdateTime = Time.time;
			Network_currentDamageAmp = owner.maxHealth / unitHealthAmount * GetValue(dmgAmpPerUnitHealth);
			numberDisplay = Mathf.RoundToInt(_currentDamageAmp * 100f);
		}
	}

	private float GetDamageAmpFallback()
	{
		if ((UnityEngine.Object)(object)DewPlayer.local == null || (UnityEngine.Object)(object)DewPlayer.local.hero == null)
		{
			return 0f;
		}
		Hero hero = DewPlayer.local.hero;
		return hero.maxHealth / unitHealthAmount * dmgAmpPerUnitHealth.GetValue(effectiveLevel, hero);
	}

	private void Amplify(ref DamageData data, Actor actor, Entity target)
	{
		if (owner.CheckEnemyOrNeutral(target) && !data.IsAmountModifiedBy(this))
		{
			data.ApplyAmplification(currentDamageAmp);
			data.SetAmountModifiedBy(this);
			NotifyUse();
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
			NetworkWriterExtensions.WriteFloat(writer, _currentDamageAmp);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _currentDamageAmp);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _currentDamageAmp, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _currentDamageAmp, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
