using Mirror;
using UnityEngine;

public class Se_MiniBoss_UnstableExplosive_Explosion : StatusEffect
{
	public float explodeDelay;

	public float maxSlow = 50f;

	public DewCollider explodeRange;

	public GameObject fxExplode;

	public GameObject fxExplodeHit;

	public ScalingValue explodeDamage;

	public Knockback explodeKnockback;

	public float selfDamageCurrentHpRatio;

	public float selfDamageMaxHpRatio;

	public float selfStunDuration;

	private SlowEffect _slow;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.Control.StartChannel(new Channel
			{
				blockedActions = (Channel.BlockedAction.Ability | Channel.BlockedAction.Attack),
				duration = explodeDelay
			});
			SetTimer(explodeDelay);
			_slow = DoSlow(0f);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (normalizedDuration.HasValue)
		{
			float t = 1f - normalizedDuration.Value;
			if (_slow != null)
			{
				_slow.strength = Mathf.Lerp(0f, maxSlow, t);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer || victim.IsNullInactiveDeadOrKnockedOut())
		{
			return;
		}
		FxPlayNetworked(fxExplode, victim);
		explodeRange.transform.position = victim.agentPosition;
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in explodeRange.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			DefaultDamage(explodeDamage).SetOriginPosition(victim.position).Dispatch(entity);
			FxPlayNewNetworked(fxExplodeHit, entity);
			explodeKnockback.ApplyWithOrigin(victim.position, entity);
		}
		handle.Return();
		if (!(victim is BossMonster))
		{
			DefaultDamage(selfDamageCurrentHpRatio * victim.currentHealth + selfDamageMaxHpRatio * victim.maxHealth).Dispatch(victim);
		}
		CreateBasicEffect(victim, new StunEffect(), selfStunDuration, "unstablestun", DuplicateEffectBehavior.UsePrevious);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_slow = null;
	}

	private void MirrorProcessed()
	{
	}
}
