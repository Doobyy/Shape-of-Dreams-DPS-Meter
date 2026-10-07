using UnityEngine;

public class Ai_C_IceBlock_Projectile : StandardProjectile
{
	public GameObject fxMisticIce;

	internal bool isMisticIce;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		isMisticIce = false;
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		if (isMisticIce)
		{
			FxPlayNetworked(fxMisticIce, hit.entity);
		}
	}

	private void MirrorProcessed()
	{
	}
}
