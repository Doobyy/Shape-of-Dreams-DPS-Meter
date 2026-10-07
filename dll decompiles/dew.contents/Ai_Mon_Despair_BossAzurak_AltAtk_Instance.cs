using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Despair_BossAzurak_AltAtk_Instance : InstantDamageInstance
{
	public GameObject fxTelegraph;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			FxPlayNetworked(fxTelegraph, info.point, null);
		}
		yield return base.OnCreateSequenced();
		if (((NetworkBehaviour)this).isServer)
		{
			yield return new SI.WaitForSeconds(1f);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
