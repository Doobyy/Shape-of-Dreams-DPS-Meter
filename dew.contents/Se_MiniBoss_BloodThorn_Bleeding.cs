using Mirror;
using UnityEngine;

public class Se_MiniBoss_BloodThorn_Bleeding : StatusEffect
{
	public float maxHealthDmgRatio = 0.15f;

	public float interval;

	public float count;

	public GameObject fxHit;

	private float _currentTime;

	private float _damage;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_damage = victim.Status.maxHealth * maxHealthDmgRatio / count;
			float timer = interval * count;
			_currentTime = Time.time;
			SetTimer(timer);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !(Time.time - _currentTime < interval))
		{
			CreateDamage(DamageData.SourceType.Pure, _damage).SetActor(info.caster).Dispatch(victim);
			FxPlayNewNetworked(fxHit, victim);
			_currentTime = Time.time;
		}
	}

	private void MirrorProcessed()
	{
	}
}
