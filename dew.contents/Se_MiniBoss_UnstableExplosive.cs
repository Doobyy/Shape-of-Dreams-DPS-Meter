using Mirror;
using UnityEngine;

public class Se_MiniBoss_UnstableExplosive : MiniBossEffect
{
	public float maxHealthBonusPercentage = 25f;

	public float explodeInterval;

	public float startDelay;

	private float _lastExplodeTime;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				maxHealthPercentage = maxHealthBonusPercentage
			});
			_lastExplodeTime = Time.time - explodeInterval + startDelay;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			if (victim.Visual.isRendererOff)
			{
				_lastExplodeTime = Time.time;
			}
			else if (!(Time.time - _lastExplodeTime < explodeInterval))
			{
				CreateStatusEffect<Se_MiniBoss_UnstableExplosive_Explosion>(victim, new CastInfo(victim));
				_lastExplodeTime = Time.time;
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_lastExplodeTime = 0f;
	}

	private void MirrorProcessed()
	{
	}
}
