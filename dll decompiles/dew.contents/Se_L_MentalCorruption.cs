using Mirror;
using UnityEngine;

public class Se_L_MentalCorruption : StackedStatusEffect
{
	public ScalingValue tickDamageAmount;

	public float tickInterval = 1f;

	public float tickProcCoefficient = 0.5f;

	public GameObject damageEffect;

	private float _lastDamageTime;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_lastDamageTime = 0f;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetStack(1);
			DestroyOnDeath(info.caster, includeKnockOuts: true);
			DestroyOnDeath(victim, includeKnockOuts: true);
			_lastDamageTime = Time.time;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		float num = 0.125f;
		float num2 = tickInterval / (float)stack;
		float num3 = Mathf.Max(num2, num);
		if (!(Time.time - _lastDamageTime > num3))
		{
			return;
		}
		_lastDamageTime += num3;
		if (!info.caster.CheckEnemyOrNeutral(victim))
		{
			Destroy();
			return;
		}
		DamageData damageData = Damage(tickDamageAmount, tickProcCoefficient).SetElemental(ElementalType.Dark);
		if (num2 < num)
		{
			float multiplier = num / num2;
			damageData.ApplyRawMultiplier(multiplier);
		}
		damageData.Dispatch(victim);
		FxPlayNetworked(damageEffect, victim);
	}

	private void MirrorProcessed()
	{
	}
}
