using Mirror;
using UnityEngine;

public class Se_FatalHitProtection_Invulnerable : StatusEffect
{
	public DewAnimationClip invulAnim;

	public float dazeDuration;

	public bool doKnockbackAndStun;

	public DewCollider range;

	public float stunDuration;

	public Knockback knockback;

	public GameObject hitEffect;

	internal float duration;

	internal string timerCustomNameKey;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		SetTimer(duration);
		ShowOnScreenTimer(timerCustomNameKey);
		DoInvulnerable();
		victim.Control.CancelOngoingDisplacement();
		victim.Control.CancelOngoingChannels();
		victim.Animation.PlayAbilityAnimation(invulAnim);
		victim.Control.StartDaze(dazeDuration);
		if (!doKnockbackAndStun)
		{
			return;
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			CreateBasicEffect(entity, new StunEffect(), stunDuration, "fatalhit_stun");
			knockback.ApplyWithOrigin(victim.position, entity);
			FxPlayNewNetworked(hitEffect, entity);
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
