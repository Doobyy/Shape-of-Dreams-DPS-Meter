using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_LavaLand_Magmadon_Charge : AbilityInstance
{
	public int projectileCount;

	public float projectileDistance;

	public ScalingValue dmgFactor;

	public Knockback knockback;

	public DewCollider range;

	public GameObject fxHit;

	public GameObject fxTelegraph;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			range.transform.position = info.caster.agentPosition;
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				FxPlayNewNetworked(fxHit, entity);
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.caster.agentPosition).SetElemental(ElementalType.Fire).Dispatch(entity);
				knockback.ApplyWithOrigin(info.caster.agentPosition, entity);
			}
			handle.Return();
			float startAngle = Random.Range(0f, 360f);
			for (int j = 0; j < projectileCount; j++)
			{
				float num = 360f / (float)projectileCount * (float)j;
				float y = startAngle + num;
				Vector3 vector = info.caster.agentPosition + Quaternion.Euler(0f, y, 0f) * info.forward * projectileDistance;
				vector = Dew.GetPositionOnGround(vector);
				CreateAbilityInstance<Ai_Mon_LavaLand_Magmadon_Charge_Projectile>(info.caster.agentPosition, null, new CastInfo(info.caster, vector));
				FxPlayNewNetworked(fxTelegraph, vector, null);
				yield return new SI.WaitForSeconds(0.1f);
			}
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
