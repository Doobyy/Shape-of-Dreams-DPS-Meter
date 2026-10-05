using UnityEngine;

public class Ai_MiniBoss_BloodThorn_Projectile : StandardProjectile
{
	public float duration;

	public override bool reuseInRoom => true;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		SetCustomStartPosition(((Component)(object)this).transform.position);
		float magnitude = (((Component)(object)this).transform.position - info.point).magnitude;
		initialSpeed = magnitude / duration;
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		CreateAbilityInstance<Ai_MiniBoss_BloodThorn_SubThorn>(info.point, null, new CastInfo(info.caster));
	}

	private void MirrorProcessed()
	{
	}
}
