using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_BossNyx_PullAtk_AfterAtk : InstantDamageInstance
{
	public float postDelay;

	public float telegraphDelay;

	public GameObject fxTelegraph;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Control.StartDaze(telegraphDelay + damageDelay + postDelay);
			yield return new SI.WaitForSeconds(telegraphDelay);
			FxPlayNetworked(fxTelegraph);
			yield return base.OnCreateSequenced();
			Destroy();
		}
	}

	protected override void OnAfterDelay()
	{
		base.OnAfterDelay();
		info.caster.Control.StartDaze(postDelay);
	}

	private void MirrorProcessed()
	{
	}
}
