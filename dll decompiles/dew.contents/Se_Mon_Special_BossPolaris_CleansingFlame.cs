using Mirror;
using UnityEngine;

public class Se_Mon_Special_BossPolaris_CleansingFlame : StatusEffect
{
	public float duration = 4f;

	public float tickInterval = 0.3333f;

	public float tickDmgMaxHpRatio = 0.01f;

	public float tickDmgCurrentHpRatio = 0.05f;

	public float tickDmgCurrentShieldRatio = 0.2f;

	public float healReduction = 0.75f;

	public float shieldReduction = 0.75f;

	private float _lastTickTime;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(duration);
			ShowOnScreenTimer(null, new Color(224f / 255f, 1f, 75f / 255f));
			_lastTickTime = Time.time;
			victim.takenHealProcessor.Add(HealProcessor);
			victim.takenShieldProcessor.Add(ShieldProcessor);
		}
	}

	private void HealProcessor(ref HealData data, Actor from, Entity to)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			data.SetAmountModifiedBy(this);
			data.ApplyReduction(healReduction);
		}
	}

	private void ShieldProcessor(ref HealData data, Actor from, Entity to)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			data.SetAmountModifiedBy(this);
			data.ApplyReduction(shieldReduction);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (bool)(Object)(object)victim)
		{
			victim.takenHealProcessor.Remove(HealProcessor);
			victim.takenShieldProcessor.Remove(ShieldProcessor);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !(Time.time - _lastTickTime < tickInterval))
		{
			_lastTickTime += tickInterval;
			float amount = tickDmgMaxHpRatio * victim.maxHealth + tickDmgCurrentHpRatio * victim.currentHealth + tickDmgCurrentShieldRatio * victim.Status.currentShield;
			DefaultDamage(amount).SetAttr(DamageAttribute.DamageOverTime).Dispatch(victim);
		}
	}

	private void MirrorProcessed()
	{
	}
}
