using Mirror;
using UnityEngine;

public class Ai_Mon_Forest_BossDemon_AltSkill_Stomp_Tree : InstantDamageInstance
{
	public GameObject fxTelegraph;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			FxPlayNetworked(fxTelegraph);
		}
	}

	private void MirrorProcessed()
	{
	}
}
