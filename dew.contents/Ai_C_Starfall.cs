using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_C_Starfall : AbilityInstance
{
	public float minStartPos;

	public float maxStartPos;

	public float interval;

	public DewCollider range;

	public GameObject shootEffect;

	public GameObject shootEffectCaster;

	public ScalingValue dmgPerStar;

	public ScalingValue baseCount;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		int count = Mathf.RoundToInt(GetValue(baseCount));
		for (int i = 0; i < count; i++)
		{
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.caster.position, range.radius, tvDefaultHarmfulEffectTargets);
			if (list.Count > 0)
			{
				Entity entity = Dew.SelectRandomWeightedInList(list, (Entity e) => e.maxHealth, null);
				Vector3 normalized = Random.insideUnitSphere.normalized;
				Vector3 vector = info.caster.Visual.GetCenterPosition() + normalized * Random.Range(minStartPos, maxStartPos);
				FxPlayNewNetworked(shootEffect, vector, Quaternion.identity);
				FxPlayNewNetworked(shootEffectCaster, info.caster);
				if (!entity.IsNullInactiveDeadOrKnockedOut())
				{
					CreateAbilityInstance(entity.position, null, new CastInfo(info.caster, entity), (Ai_C_Starfall_Projectile b) =>
					{
						b.dmgFactor = dmgPerStar;
					});
				}
			}
			handle.Return();
			yield return new SI.WaitForSeconds(interval);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
