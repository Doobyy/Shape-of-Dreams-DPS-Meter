using System;
using Mirror;
using UnityEngine;

public class Se_GenericHealOverTime : StatusEffect
{
	public float totalAmount;

	public int ticks;

	public float tickInterval;

	public GameObject perHealEffect;

	private int _doneTicks;

	private float _nextTickTime;

	public override bool reuseInRoom => true;

	public void Setup(float totalAmount, float tickInterval, int ticks)
	{
		this.totalAmount = totalAmount;
		this.tickInterval = tickInterval;
		this.ticks = ticks;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_doneTicks = 0;
			_nextTickTime = Time.time;
			ProcessTicks();
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && isActive)
		{
			ProcessTicks();
		}
	}

	private void ProcessTicks()
	{
		while (_doneTicks < ticks && Time.time >= _nextTickTime)
		{
			if (victim.Status.isDead || !victim.isActive || victim is Hero { isKnockedOut: not false })
			{
				Destroy();
				return;
			}
			_nextTickTime += tickInterval;
			FxPlayNetworked(perHealEffect, victim);
			if (ticks <= 1 || Math.Abs(victim.currentHealth - victim.maxHealth) > 0.0001f)
			{
				HealData heal = new HealData(totalAmount / (float)ticks);
				heal.SetCanMerge();
				DoHeal(heal, victim, chain);
			}
			_doneTicks++;
		}
		if (_doneTicks >= ticks)
		{
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
