using System;
using Mirror;
using UnityEngine;

public class Gem_R_Hedgehog : Gem
{
	public float empowerDuration = 4f;

	private float _lastEmpoweredTime;

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		newOwner.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if ((UnityEngine.Object)(object)oldOwner != null)
		{
			oldOwner.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	public bool IsEmpowered()
	{
		return Time.time - _lastEmpoweredTime < empowerDuration;
	}

	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		NotifyUse();
		if (owner.Status.TryGetStatusEffect<Se_Gem_R_Hedgehog_Aura>(out var effect))
		{
			effect.ResetTimer();
		}
		else
		{
			CreateStatusEffectWithSource<Se_Gem_R_Hedgehog_Aura>(info.instance, owner, new CastInfo(owner));
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		_lastEmpoweredTime = Time.time;
		NotifyUse();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			if (IsEmpowered())
			{
				fillAmount = 1f - (Time.time - _lastEmpoweredTime) / empowerDuration;
			}
			else
			{
				fillAmount = 0f;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
