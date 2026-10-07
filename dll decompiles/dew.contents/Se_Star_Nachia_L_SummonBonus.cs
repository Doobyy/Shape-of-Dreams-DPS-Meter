using System;
using Mirror;
using UnityEngine;

public class Se_Star_Nachia_L_SummonBonus : StarEffect
{
	public StarScalingValue armorAmount;

	public StarScalingValue healShieldAmp;

	public float checkInterval = 0.5f;

	public float checkRadius = 6.5f;

	private float _lastCheckTime;

	private ArmorBoostEffect _armor;

	public override Type heroType => typeof(Hero_Nachia);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.dealtHealProcessor.Add(SummonAmp);
			hero.dealtShieldProcessor.Add(SummonAmp);
			_armor = DoArmorBoost(0f);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || _armor == null || Time.time - _lastCheckTime < checkInterval)
		{
			return;
		}
		bool flag = false;
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, hero.agentPosition, checkRadius))
		{
			if (item is Summon)
			{
				flag = true;
				break;
			}
		}
		handle.Return();
		if (flag && _armor.strength <= 0.01f)
		{
			_armor.strength = GetValue(armorAmount);
		}
		else if (!flag && _armor.strength > 0f)
		{
			_armor.strength = 0f;
		}
	}

	private void SummonAmp(ref HealData data, Actor actor, Entity target)
	{
		if (target is Summon && !data.IsAmountModifiedBy(this))
		{
			data.SetAmountModifiedBy(this);
			data.ApplyAmplification(GetValue(healShieldAmp));
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.dealtHealProcessor.Remove(SummonAmp);
			hero.dealtShieldProcessor.Remove(SummonAmp);
		}
	}

	private void MirrorProcessed()
	{
	}
}
