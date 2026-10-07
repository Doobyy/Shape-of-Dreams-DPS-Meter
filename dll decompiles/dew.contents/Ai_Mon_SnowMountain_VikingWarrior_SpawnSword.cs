using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_SnowMountain_VikingWarrior_SpawnSword : AbilityInstance
{
	public float delay;

	public GameObject fxTelegraph;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			FxPlayNetworked(fxTelegraph, info.point, null);
			info.caster.Control.StartDaze(delay);
			yield return new SI.WaitForSeconds(delay);
			CreateAbilityInstance<Ai_Mon_SnowMountain_VikingWarrior_Sword>(info.point, null, info);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
