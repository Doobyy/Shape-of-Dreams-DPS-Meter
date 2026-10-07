using Mirror;
using UnityEngine;

public class Se_Elm_Fire : ElementalStatusEffect
{
	public float heroMultiplier;

	public float bossMultiplier;

	private float _lastDamageTick;

	private float _damageMultiplier;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_damageMultiplier = 0f;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_lastDamageTick = Time.time;
			DoFireDamage();
			victim.Status.fireStack = stack;
			if (victim is Hero hero)
			{
				_damageMultiplier = 1f + hero.Status.scalingStats.maxHealthPercentage * 0.01f * (float)hero.Status.level * 2f;
				decayTime *= 0.7f;
				ResetDecayTimer();
			}
			else if (victim is BossMonster)
			{
				_damageMultiplier = NetworkedManagerBase<GameManager>.instance.GetBossMonsterHealthMultiplierByScaling();
			}
			else if (victim is Monster { type: Monster.MonsterType.MiniBoss })
			{
				_damageMultiplier = NetworkedManagerBase<GameManager>.instance.GetMiniBossMonsterHealthMultiplierByScaling();
			}
			else
			{
				_damageMultiplier = NetworkedManagerBase<GameManager>.instance.GetRegularMonsterHealthMultiplierByScaling();
			}
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !(victim is Monster { isSleeping: not false }) && Time.time - _lastDamageTick > 0.25f)
		{
			_lastDamageTick += 0.25f;
			DoFireDamage();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((Object)(object)victim != null && victim.Status.isAlive && Time.time - _lastDamageTick > 0.125f)
			{
				DoFireDamage();
			}
			if ((Object)(object)victim != null && victim.Status.isAlive)
			{
				victim.Status.fireStack = 0;
			}
		}
	}

	protected override void OnStackChange(int oldStack, int newStack)
	{
		base.OnStackChange(oldStack, newStack);
		if ((newStack != 0 || !victim.Status.isDead) && ((NetworkBehaviour)this).isServer)
		{
			victim.Status.fireStack = newStack;
		}
	}

	private void DoFireDamage()
	{
		float damageMultiplier = _damageMultiplier;
		damageMultiplier *= 1f + (float)(stack - 1) * 0.29999995f;
		DamageData damage = new DamageData(DamageData.SourceType.Default, 3.5f * damageMultiplier, 0f);
		damage.ApplyAmplification(ampAmount);
		if (victim is Hero)
		{
			damage.ApplyRawMultiplier(heroMultiplier);
		}
		if (victim is Monster { type: Monster.MonsterType.Boss })
		{
			damage.ApplyRawMultiplier(bossMultiplier);
		}
		damage.SetAttr(DamageAttribute.DamageOverTime);
		DealDamage(damage, victim);
	}

	private void MirrorProcessed()
	{
	}
}
