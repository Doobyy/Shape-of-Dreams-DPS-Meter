using Mirror;
using UnityEngine;

public class Ai_Mon_Forest_BossDemon_AltSkill_LineAtk : InstantDamageInstance
{
	public GameObject fxReady;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			FxPlayNetworked(fxReady, position, rotation);
		}
	}

	private void MirrorProcessed()
	{
	}
}
