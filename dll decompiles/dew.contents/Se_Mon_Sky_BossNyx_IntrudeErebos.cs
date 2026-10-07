using System.Collections;
using Mirror;
using UnityEngine;

public class Se_Mon_Sky_BossNyx_IntrudeErebos : StatusEffect
{
	public float staggerDuration;

	public DewAnimationClip staggerClip;

	public DewAnimationClip endClip;

	public GameObject staggerEffect;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DoInvulnerable();
			DoUncollidable();
			DoUntargetable();
			info.caster.Control.Stop();
			info.caster.Control.CancelOngoingChannels();
			info.caster.Control.StartDaze(staggerDuration);
			info.caster.Animation.PlayAbilityAnimation(staggerClip);
			FxPlayNetworked(staggerEffect, victim);
			yield return new SI.WaitForSeconds(4f);
			info.caster.Animation.PlayAbilityAnimation(endClip);
			yield return new SI.WaitForSeconds(staggerDuration);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(staggerEffect);
		}
	}

	private void MirrorProcessed()
	{
	}
}
