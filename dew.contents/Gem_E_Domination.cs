using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Gem_E_Domination : Gem
{
	public GameObject fxHit;

	public GameObject[] fxCharged;

	public float critChanceBonusAmount;

	public float critAmpBonusValue;

	public ScalingValue critAmpMaxValue;

	[NonSerialized]
	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public float gainedCritAmp;

	private float _critDamageAmp;

	private StatBonus _bonus;

	private GameObject _currentChargeEffect;

	public float NetworkgainedCritAmp
	{
		get
		{
			return gainedCritAmp;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref gainedCritAmp, 262144uL, (Action<float, float>)null);
		}
	}

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = new StatBonus();
			_bonus.critChanceFlat = critChanceBonusAmount;
			_bonus.critAmpFlat = Mathf.Min(gainedCritAmp, GetValue(critAmpMaxValue));
			NetworkgainedCritAmp = _bonus.critAmpFlat;
			if (!((UnityEngine.Object)(object)newOwner == null))
			{
				newOwner.EntityEvent_OnAttackHit += new Action<EventInfoAttackHit>(CheckCritical);
				newOwner.Status.AddStatBonus(_bonus);
				numberDisplay = Mathf.RoundToInt(gainedCritAmp * 100f);
				PlayChargeEffect();
			}
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)oldOwner != null)
		{
			oldOwner.Status.RemoveStatBonus(_bonus);
			oldOwner.EntityEvent_OnAttackHit -= new Action<EventInfoAttackHit>(CheckCritical);
			FxStopNetworked(_currentChargeEffect);
		}
	}

	private void CheckCritical(EventInfoAttackHit obj)
	{
		if (!obj.isCrit)
		{
			return;
		}
		if (_bonus.critAmpFlat >= GetValue(critAmpMaxValue))
		{
			_bonus.critAmpFlat = GetValue(critAmpMaxValue);
			FxPlayNetworked(fxHit, obj.victim);
		}
		else
		{
			_bonus.critAmpFlat += critAmpBonusValue;
			for (int i = 0; i < UnityEngine.Random.Range(2, 4); i++)
			{
				bool enableSfx = i == 0;
				CreateAbilityInstance(obj.victim.position, null, new CastInfo(owner, owner), (Ai_Gem_E_Domination_CritGainEffect b) =>
				{
					b.SetCustomStartPosition(obj.victim.Visual.GetCenterPosition());
					b.enableEntitySound = enableSfx;
				});
			}
		}
		NetworkgainedCritAmp = _bonus.critAmpFlat;
		numberDisplay = Mathf.RoundToInt(gainedCritAmp * 100f);
		PlayChargeEffect();
		NotifyUse();
	}

	private void PlayChargeEffect()
	{
		FxStopNetworked(_currentChargeEffect);
		float num = gainedCritAmp / GetValue(critAmpMaxValue);
		if (num >= 0.9f)
		{
			_currentChargeEffect = fxCharged[2];
			FxPlayNetworked(_currentChargeEffect, owner);
		}
		else if (num >= 0.5f)
		{
			_currentChargeEffect = fxCharged[1];
			FxPlayNetworked(_currentChargeEffect, owner);
		}
		else if (num >= 0.1f)
		{
			_currentChargeEffect = fxCharged[0];
			FxPlayNetworked(_currentChargeEffect, owner);
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
			NetworkWriterExtensions.WriteFloat(writer, gainedCritAmp);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, gainedCritAmp);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref gainedCritAmp, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref gainedCritAmp, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
