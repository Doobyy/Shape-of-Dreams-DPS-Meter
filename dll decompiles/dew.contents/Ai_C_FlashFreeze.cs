using UnityEngine;

public class Ai_C_FlashFreeze : InstantDamageInstance
{
	public GameObject fxEmpoweredHit;

	public float damageAmpChilled;

	public ScalingValue shieldAmount;

	public float shieldDuration = 2f;

	public override bool reuseInRoom => true;

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		if (target.Status.hasCold)
		{
			if (target.Status.TryGetStatusEffect<Se_C_FlashFreeze_Root>(out var effect))
			{
				effect.ResetTimer();
			}
			else if (!target.Status.hasCrowdControlImmunity)
			{
				CreateStatusEffect<Se_C_FlashFreeze_Root>(target);
			}
			dmg.SetAttr(DamageAttribute.IsCrit);
			dmg.ApplyAmplification(damageAmpChilled);
			FxPlayNewNetworked(fxEmpoweredHit, target);
		}
	}

	protected override void OnCollisionCheck()
	{
		base.OnCollisionCheck();
		GiveShield(info.caster, GetValue(shieldAmount), shieldDuration);
		FxPlayNewNetworked(hitEffect, info.caster);
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultUsefulEffectTargets, new CollisionCheckSettings
		{
			includeUncollidable = true
		}))
		{
			if (!((Object)(object)entity == (Object)(object)info.caster))
			{
				GiveShield(entity, GetValue(shieldAmount), shieldDuration);
				FxPlayNewNetworked(hitEffect, entity);
			}
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
