using Mirror;
using UnityEngine;

public class Ai_Q_Laceration_Dash : DashAttackInstance
{
	public DewAnimationClip anim;

	public float cooldownReductionRatio = 0.35f;

	private bool _didHit;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_didHit = false;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Animation.PlayAbilityAnimation(anim);
			CreateBasicEffect(info.caster, new UncollidableEffect(), 0.35f);
			CreateBasicEffect(info.caster, new UnstoppableEffect(), 0.35f);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		_ = ((NetworkBehaviour)this).isServer;
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		if (!_didHit)
		{
			_didHit = true;
			if ((Object)(object)firstTrigger != null)
			{
				ApplyCooldownReductionByRatio(firstTrigger, cooldownReductionRatio);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
