using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_DivineAnimal_Stomp : AbilityInstance
{
	public GameObject telegraph;

	public float spawnDuration;

	public DewCollider range;

	public AbilityTargetValidator hittable;

	public ScalingValue dmgFactor;

	public float knockupStrengh;

	public Knockback knockback;

	public GameObject landEffect;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			range.transform.position = Dew.GetPositionOnGround(range.transform.position);
			FxPlayNetworked(telegraph);
			yield return new SI.WaitForSeconds(spawnDuration);
			FxPlayNetworked(landEffect);
			List<Entity> entities = range.GetEntities(out var handle, hittable, info.caster);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				entity.Visual.KnockUp(knockupStrengh, isFriendly: false);
				knockback.ApplyWithOrigin(info.caster.agentPosition, entity);
				CreateDamage(DamageData.SourceType.Default, dmgFactor).Dispatch(entity);
			}
			handle.Return();
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
