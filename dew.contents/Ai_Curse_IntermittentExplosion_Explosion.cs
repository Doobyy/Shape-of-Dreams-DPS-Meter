using Mirror;
using UnityEngine;

public class Ai_Curse_IntermittentExplosion_Explosion : InstantDamageInstance, IOtherPlayersTonedDownDisable
{
	public override bool reuseInRoom => true;

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			CreateBasicEffect(info.caster, new SlowEffect
			{
				decay = true,
				strength = 50f
			}, 1f, "IntermittentSlow", DuplicateEffectBehavior.UsePrevious);
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		position = info.caster.agentPosition;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && Se_Curse_IntermittentExplosion.ShouldBeDestroyed())
		{
			Destroy();
		}
	}

	protected override bool ShouldUseDefaultAbilityTargetValidator()
	{
		return false;
	}

	protected override bool OnValidateTarget(Entity entity)
	{
		return (Object)(object)info.caster != (Object)(object)entity;
	}

	private void MirrorProcessed()
	{
	}
}
