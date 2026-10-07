using System.Collections.Generic;

public class Ai_Mon_Special_BossMaw_ChainReaction_Instance : TickDamageInstance
{
	public float healAmount;

	protected override void OnCreate()
	{
		position = info.point;
		base.OnCreate();
	}

	protected override void OnCollisionCheck()
	{
		base.OnCollisionCheck();
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		int num = 0;
		foreach (Entity item in entities)
		{
			if (!item.IsNullInactiveDeadOrKnockedOut() && !item.Status.hasDamageImmunity)
			{
				num++;
			}
		}
		if (num <= 0)
		{
			handle.Return();
			return;
		}
		float amount = info.caster.Status.missingHealth * (healAmount * (float)num);
		if (!range.GetEntities(out handle, tvDefaultUsefulEffectTargets).Contains(info.caster))
		{
			handle.Return();
			return;
		}
		Heal(amount).SetCanMerge().Dispatch(info.caster);
		FxPlayNewNetworked(hitEffect, info.caster);
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
