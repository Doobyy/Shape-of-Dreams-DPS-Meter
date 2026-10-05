using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_C_Purgatory : AbilityInstance
{
	public float delay;

	public GameObject explodeEffect;

	public ScalingValue damage;

	public GameObject hitEffect;

	public DewCollider range;

	public float knockupStrength = 0.7f;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		((Component)(object)this).transform.SetPositionAndRotation(info.point, Quaternion.identity);
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		yield return new SI.WaitForSeconds(delay);
		FxPlayNetworked(explodeEffect);
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, ((Component)(object)this).transform.position, 20f, tvDefaultUsefulEffectTargets, new CollisionCheckSettings
		{
			includeUncollidable = true
		});
		List<Entity> entities = range.GetEntities(out var handle2, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity v = entities[i];
			v.Visual.KnockUp(knockupStrength, isFriendly: false);
			Damage(damage).SetDirection(Vector3.up).Dispatch(v);
			FxPlayNewNetworked(hitEffect, v);
			for (int j = 0; j < ((!v.IsAnyBoss()) ? 1 : 3); j++)
			{
				Entity entity = Dew.SelectRandomWeightedInList(list, (Entity e) => Mathf.Clamp(1f - e.currentHealth / e.maxHealth, 1E-05f, 1f) * (float)((!(e is Hero)) ? 1 : 4), null);
				if (!entity.IsNullInactiveDeadOrKnockedOut())
				{
					CreateAbilityInstance(((Component)(object)this).transform.position, null, new CastInfo(info.caster, entity), (Ai_C_Purgatory_Heal heal) =>
					{
						heal.SetCustomStartPosition(v.Visual.GetCenterPosition());
					});
				}
			}
		}
		handle2.Return();
		handle.Return();
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
