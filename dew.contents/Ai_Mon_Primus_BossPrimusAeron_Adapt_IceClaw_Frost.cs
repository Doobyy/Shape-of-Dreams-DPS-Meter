using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Primus_BossPrimusAeron_Adapt_IceClaw_Frost : AbilityInstance
{
	public float delay = 0.5f;

	public float stunDuration = 1f;

	public ScalingValue damage;

	public GameObject fxHit;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			yield return new SI.WaitForSeconds(delay);
			Damage(damage).Dispatch(info.target);
			FxPlayNetworked(fxHit, info.target);
			CreateBasicEffect(info.target, new StunEffect(), stunDuration);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
