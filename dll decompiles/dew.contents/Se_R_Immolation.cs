using Mirror;
using UnityEngine;

public class Se_R_Immolation : StatusEffect
{
	public ScalingValue speedAmount;

	public ScalingValue sacrificedHpRatio;

	public float duration = 3f;

	public int selfDamageTicks = 5;

	private int _remainingSelfDamageTicks;

	private float _lastSelfDamageTime;

	private float _sacrificedHealth;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_lastSelfDamageTime = 0f;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoSpeed(GetValue(speedAmount));
			SetTimer(duration);
			ShowOnScreenTimer();
			CreateAbilityInstance<Ai_R_Immolation_Damage>(victim.position, null, new CastInfo(victim));
			_remainingSelfDamageTicks = selfDamageTicks;
			_sacrificedHealth = victim.currentHealth * GetValue(sacrificedHpRatio);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && Time.time - _lastSelfDamageTime > duration / (float)selfDamageTicks && _remainingSelfDamageTicks > 0)
		{
			_lastSelfDamageTime = Time.time;
			DoTick();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !victim.IsNullInactiveDeadOrKnockedOut())
		{
			while (_remainingSelfDamageTicks > 0)
			{
				DoTick();
			}
		}
	}

	private void DoTick()
	{
		PureDamage(_sacrificedHealth / (float)selfDamageTicks).SetAttr(DamageAttribute.IgnoreArmor).SetAttr(DamageAttribute.IgnoreShield).SetAttr(DamageAttribute.IgnoreDamageImmunity)
			.SetAttr(DamageAttribute.DamageOverTime)
			.Dispatch(victim);
		_remainingSelfDamageTicks--;
	}

	private void MirrorProcessed()
	{
	}
}
