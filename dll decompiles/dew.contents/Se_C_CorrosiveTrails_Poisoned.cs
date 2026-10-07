using Mirror;
using UnityEngine;

public class Se_C_CorrosiveTrails_Poisoned : StatusEffect
{
	public ScalingValue perTickDamage;

	public float tickInterval = 0.3333f;

	public float poisonDuration = 3f;

	public GameObject fxPerTickEffect;

	private float _lastTickTime;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_lastTickTime = 0f;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(poisonDuration + 0.1f);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && Time.time - _lastTickTime > tickInterval)
		{
			_lastTickTime = Time.time;
			FxPlayNewNetworked(fxPerTickEffect, victim);
			Damage(perTickDamage, 0.5f).SetAttr(DamageAttribute.ForceMergeNumber).SetAttr(DamageAttribute.DamageOverTime).Dispatch(victim);
		}
	}

	private void MirrorProcessed()
	{
	}
}
