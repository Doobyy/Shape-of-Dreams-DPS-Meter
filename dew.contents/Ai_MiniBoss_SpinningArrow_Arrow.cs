using Mirror;
using UnityEngine;

public class Ai_MiniBoss_SpinningArrow_Arrow : StandardProjectile
{
	private class Ad_SpinningArrowHit
	{
		public float lastMainHitTime;
	}

	public ScalingValue dmg;

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
		if (!hit.entity.TryGetData<Ad_SpinningArrowHit>(out var data))
		{
			data = new Ad_SpinningArrowHit();
			hit.entity.AddData(data);
		}
		float num = 1f;
		if (Time.time - data.lastMainHitTime > 2f)
		{
			num *= 1.65f;
			data.lastMainHitTime = Time.time;
		}
		else
		{
			num *= 0.25f;
		}
		Damage(dmg).ApplyRawMultiplier(num).SetDirection(rotation).Dispatch(hit.entity);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
