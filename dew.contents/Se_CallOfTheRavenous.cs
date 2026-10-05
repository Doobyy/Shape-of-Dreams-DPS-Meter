using System.Collections;
using Mirror;
using UnityEngine;

public class Se_CallOfTheRavenous : StatusEffect
{
	public float delay;

	public GameObject fxKill;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(victim);
			yield return new SI.WaitForSeconds(delay);
			FxPlayNetworked(fxKill, victim);
			victim.Kill();
			DestroyIfActive();
		}
	}

	private void MirrorProcessed()
	{
	}
}
