using Mirror;
using UnityEngine;

public class Se_Treasure_FragmentOfDetermination_Invulnerable : StatusEffect
{
	public float dazeDuration;

	public bool doKnockbackAndStun;

	public DewCollider range;

	public float stunDuration;

	public Knockback knockback;

	public GameObject hitEffect;

	public float duration;

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
		victim.Control.StartDaze(dazeDuration);
		if (!doKnockbackAndStun)
		{
			return;
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			CreateBasicEffect(entity, new StunEffect(), stunDuration);
			knockback.ApplyWithOrigin(victim.position, entity);
			FxPlayNewNetworked(hitEffect, entity);
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
