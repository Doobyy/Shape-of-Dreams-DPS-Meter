using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_R_Inspire_Roar : AbilityInstance
{
	public DewCollider range;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultUsefulEffectTargets, new CollisionCheckSettings
		{
			includeUncollidable = true
		});
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			List<Se_R_Inspire_Buff> combinableList = DewPool.GetList(out ListReturnHandle<Se_R_Inspire_Buff> handle2);
			foreach (StatusEffect statusEffect in entity.Status.statusEffects)
			{
				if (statusEffect is Se_R_Inspire_Buff se_R_Inspire_Buff && !((Object)(object)se_R_Inspire_Buff.parentActor.parentActor != (Object)(object)parentActor) && se_R_Inspire_Buff.givingTo.IsNullOrInactive())
				{
					combinableList.Add(se_R_Inspire_Buff);
				}
			}
			Se_R_Inspire_Buff actor = CreateStatusEffect(entity, new CastInfo(info.caster), (Se_R_Inspire_Buff se) =>
			{
				if (combinableList.Count > 0 && Random.value < (float)combinableList.Count * 0.15f)
				{
					Se_R_Inspire_Buff se_R_Inspire_Buff2 = combinableList[Random.Range(0, combinableList.Count)];
					se_R_Inspire_Buff2.givingTo = se;
					se.receivingFrom = se_R_Inspire_Buff2;
				}
			});
			if (i == 0)
			{
				DestroyOnDestroy(actor);
			}
			handle2.Return();
		}
		ResetCooldown(info.caster.Ability.attackAbility);
		handle.Return();
		if (entities.Count <= 0)
		{
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
