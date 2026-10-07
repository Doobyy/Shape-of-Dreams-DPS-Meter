using UnityEngine;

public class Ai_R_Lava_LavaField : TickDamageInstance
{
	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		ClientActorEvent_OnDestroyed = null;
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		foreach (StatusEffect statusEffect in entity.Status.statusEffects)
		{
			if (statusEffect is Se_R_Lava_Debuff se_R_Lava_Debuff && (Object)(object)se_R_Lava_Debuff.info.caster == (Object)(object)info.caster)
			{
				se_R_Lava_Debuff.ResetTimer();
				return;
			}
		}
		CreateStatusEffect<Se_R_Lava_Debuff>(entity, new CastInfo(info.caster));
	}

	private void MirrorProcessed()
	{
	}
}
