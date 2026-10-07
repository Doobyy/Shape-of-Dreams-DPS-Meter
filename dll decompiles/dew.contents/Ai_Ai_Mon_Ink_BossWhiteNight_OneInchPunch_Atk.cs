using Mirror;
using UnityEngine;

public class Ai_Ai_Mon_Ink_BossWhiteNight_OneInchPunch_Atk : InstantDamageInstance
{
	public float postDelay;

	public GameObject fxTelegraph;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			FxPlayNetworked(fxTelegraph);
		}
		base.OnCreate();
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
