using Mirror;
using UnityEngine;

public class Se_MiniBoss_BloodThorn : MiniBossEffect
{
	public float maxHealthReducePercentage = -10f;

	public float baseAtkInterval;

	public float startDelay;

	public float randomMag;

	public GameObject fxSpawn;

	private float _lastAtkTime;

	private float _atkInterval;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				maxHealthPercentage = maxHealthReducePercentage
			});
			_atkInterval = baseAtkInterval + Random.Range(0f - randomMag, randomMag);
			_lastAtkTime = Time.time - _atkInterval + startDelay;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			if (victim.Visual.isRendererOff)
			{
				_lastAtkTime = Time.time;
			}
			else if (!(Time.time - _lastAtkTime < _atkInterval))
			{
				_atkInterval = baseAtkInterval + Random.Range(0f - randomMag, randomMag);
				_lastAtkTime = Time.time;
				CreateStatusEffect<Se_MiniBoss_BloodThorn_ThornSpawner>(victim, new CastInfo(victim, victim));
				FxPlayNetworked(fxSpawn, victim);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
