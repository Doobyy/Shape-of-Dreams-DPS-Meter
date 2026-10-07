using UnityEngine;

public class Ai_Mon_Special_BossPolaris_Holy_Purgatory_Instance : InstantDamageInstance
{
	protected override void OnAfterDelay()
	{
		base.OnAfterDelay();
		CreateAbilityInstance<Ai_Mon_Special_BossPolaris_Holy_Purgatory_BurningFloor>(position, null, new CastInfo(info.caster));
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		if (entity.Status.hasDamageImmunity)
		{
			return;
		}
		if (entity.Status.TryGetStatusEffect<Se_Mon_Special_BossPolaris_CleansingFlame>(out var effect))
		{
			effect.ResetTimer();
		}
		else
		{
			CreateStatusEffect<Se_Mon_Special_BossPolaris_CleansingFlame>(entity);
		}
		if (!(entity is Summon))
		{
			Vector3 startPos = entity.Visual.GetCenterPosition();
			CreateAbilityInstance(startPos, null, new CastInfo(info.caster, info.caster), (Ai_Mon_Special_BossPolaris_Holy_Purgatory_HealProjectile ai) =>
			{
				ai.SetCustomStartPosition(startPos);
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
