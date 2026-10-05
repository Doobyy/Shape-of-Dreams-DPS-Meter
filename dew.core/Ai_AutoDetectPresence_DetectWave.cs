using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_AutoDetectPresence_DetectWave : AbilityInstance
{
	public float delay = 0.75f;

	public GameObject fxAfterDelay;

	public GameObject fxHit;

	public float revealDuration = 5f;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		yield return new SI.WaitForSeconds(delay);
		FxPlayNetworked(fxAfterDelay, info.caster);
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			if (!allHero.IsNullInactiveDeadOrKnockedOut())
			{
				Reveal(allHero, revealDuration);
				FxPlayNewNetworked(fxHit, allHero);
			}
		}
		info.caster.Control.StartDaze(1f);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
