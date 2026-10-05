using System;
using Mirror;
using UnityEngine;

public class Se_Mon_Ink_Boss_Rage : StatusEffect
{
	public float atkSpeedPercentage;

	public float moveSpeedPercentage;

	public float abilityHastePercentage;

	public float dmgBoostRatio;

	public float armorAmount;

	private ParticleSystem[] _particleSystems;

	protected override void OnCreate()
	{
		base.OnCreate();
		_particleSystems = startEffectVictim.GetComponentsInChildren<ParticleSystem>();
		victim.Visual.ClientEvent_OnRendererEnabledChanged += new Action<bool>(OnRendererEnableChanged);
		if (((NetworkBehaviour)this).isServer)
		{
			DoUnstoppable();
			victim.Status.AddStatBonus(new StatBonus
			{
				abilityHastePercentage = abilityHastePercentage,
				attackSpeedPercentage = atkSpeedPercentage,
				movementSpeedPercentage = moveSpeedPercentage,
				armorFlat = armorAmount
			});
			victim.dealtDamageProcessor.Add(AmplifyDamage);
		}
	}

	private void AmplifyDamage(ref DamageData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			data.SetAmountModifiedBy(this);
			data.ApplyAmplification(dmgBoostRatio);
		}
	}

	private void OnRendererEnableChanged(bool obj)
	{
		if (victim.Visual.isRendererOff)
		{
			ParticleSystem[] particleSystems = _particleSystems;
			for (int i = 0; i < particleSystems.Length; i++)
			{
				particleSystems[i].Clear();
			}
		}
		else
		{
			ParticleSystem[] particleSystems = _particleSystems;
			for (int i = 0; i < particleSystems.Length; i++)
			{
				particleSystems[i].Play();
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		victim.Visual.ClientEvent_OnRendererEnabledChanged -= new Action<bool>(OnRendererEnableChanged);
		if (((NetworkBehaviour)this).isServer)
		{
			victim.dealtDamageProcessor.Remove(AmplifyDamage);
		}
	}

	private void MirrorProcessed()
	{
	}
}
