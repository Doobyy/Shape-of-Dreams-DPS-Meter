using System;
using Mirror;

public class Se_GenericShield_OneShot : StatusEffect
{
	public bool isDecay = true;

	[NonSerialized]
	public float initAmount;

	public ShieldEffect shield;

	private float _lastNormalizedDuration;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		_lastNormalizedDuration = 0f;
		if (((NetworkBehaviour)this).isServer)
		{
			shield = DoShield(initAmount, OnDamageNegated);
			if (shield.amount < 0.0001f)
			{
				Destroy();
			}
		}
	}

	public void SetAmount(float amount)
	{
		amount = ProcessShieldAmount(amount, victim);
		shield.amount = amount;
	}

	public void AddAmount(float amount)
	{
		amount = ProcessShieldAmount(amount, victim);
		shield.amount += amount;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && isDecay && normalizedDuration.HasValue)
		{
			if (normalizedDuration.Value < _lastNormalizedDuration)
			{
				shield.amount = shield.amount * normalizedDuration.Value / _lastNormalizedDuration;
			}
			_lastNormalizedDuration = normalizedDuration.Value;
		}
	}

	private void OnDamageNegated(EventInfoDamageNegatedByShield obj)
	{
		if (obj.shield.amount < 0.0001f)
		{
			DestroyIfActive();
		}
	}

	private void MirrorProcessed()
	{
	}
}
