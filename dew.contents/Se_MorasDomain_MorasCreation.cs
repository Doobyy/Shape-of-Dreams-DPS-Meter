using UnityEngine;

public class Se_MorasDomain_MorasCreation : StatusEffect
{
	private static readonly int EmissionColor;

	public GameObject fxDeathEffect;

	public GameObject fxTakeDamage;

	public GameObject fxTakeDamageDoT;

	public float takeDamageInterval;

	private float _lastTakeDamage;

	private float _lastTakeDamageDoT;

	public override bool reuseInRoom
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	protected override void OnCreate()
	{
	}

	protected override void OnDestroyActor()
	{
	}

	protected override void OnDisable()
	{
	}

	protected virtual void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
	}

	private void EntityEventOnDeath(EventInfoKill obj)
	{
	}

	private void MirrorProcessed()
	{
	}
}
