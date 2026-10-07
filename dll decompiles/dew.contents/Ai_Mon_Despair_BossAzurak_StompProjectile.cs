using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Despair_BossAzurak_StompProjectile : AbilityInstance
{
	public int projectileSpawnCount;

	public float dmgDelay;

	public ScalingValue dmgFactor;

	public DewCollider range;

	public Knockback knockback;

	public GameObject fxTelegraph;

	public GameObject fxMain;

	public GameObject fxHit;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		FxPlayNetworked(fxTelegraph, info.point, Quaternion.identity);
		yield return new SI.WaitForSeconds(dmgDelay);
		FxStopNetworked(fxTelegraph);
		FxPlayNetworked(fxMain, info.point, Quaternion.identity);
		range.transform.position = info.point;
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.point).Dispatch(entity);
			knockback.ApplyWithOrigin(info.point, entity);
			FxPlayNewNetworked(fxHit, entity);
		}
		handle.Return();
		int num = 360 / projectileSpawnCount;
		for (int i = 0; i < projectileSpawnCount; i++)
		{
			Quaternion quaternion = Quaternion.AngleAxis(num * i, Vector3.up);
			CreateAbilityInstance(info.point, null, new CastInfo(info.caster, CastInfo.GetAngle(quaternion)), (Ai_Mon_Despair_BossAzurak_StompProjectile_Projectile b) =>
			{
				b.SetCustomStartPosition(info.point);
			});
		}
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxTelegraph);
		}
	}

	private void MirrorProcessed()
	{
	}
}
