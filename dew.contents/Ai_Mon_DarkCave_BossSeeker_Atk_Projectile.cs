using UnityEngine;

public class Ai_Mon_DarkCave_BossSeeker_Atk_Projectile : StandardProjectile
{
	public float startHeight;

	public float frontDist;

	public override bool reuseInRoom => true;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		Vector3 normalized = (info.point - info.caster.agentPosition).Flattened().normalized;
		SetCustomStartPosition(info.caster.position + Vector3.up * startHeight + normalized * frontDist);
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		CreateAbilityInstance<Ai_Mon_DarkCave_BossSeeker_Atk_DelayedExplosion>(info.point, null, new CastInfo(info.caster, info.point));
	}

	private void MirrorProcessed()
	{
	}
}
