using System.Collections;
using Mirror;

public class Ai_Mon_Special_BossPolaris_Holy_GoldenSpear_Projectile : StandardProjectile
{
	public ScalingValue damage;

	protected override IEnumerator OnCreateSequenced()
	{
		_ = (bool)effectOnFly;
		if (((NetworkBehaviour)this).isServer)
		{
			yield return new SI.WaitForSeconds(0.15f);
			canCollideMidFlight = true;
		}
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(damage).SetDirection(rotation).Dispatch(hit.entity);
	}

	private void MirrorProcessed()
	{
	}
}
