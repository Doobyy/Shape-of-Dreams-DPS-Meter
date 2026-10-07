using Mirror;
using UnityEngine;

public class Ai_Mon_Forest_BossDemon_MainSkill_SpawnMissiles_Missile : StandardProjectile
{
	public ScalingValue damage;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
		}
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		DefaultDamage(damage).SetDirection(((Component)(object)this).transform.forward).Dispatch(hit.entity);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
