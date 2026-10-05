using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_BossNyx_LaserAtk_Instance : InstantDamageInstance
{
	public GameObject fxTelegraph;

	public GameObject fxExplosion;

	public override bool reuseInRoom => true;

	public override bool reuseInRoomSkipPrewarmCap => true;

	protected override void OnCreate()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			FxPlayNetworked(fxTelegraph, position, Quaternion.identity);
		}
		base.OnCreate();
	}

	protected override void OnAfterDelay()
	{
		base.OnAfterDelay();
		FxPlayNetworked(fxExplosion, position, Quaternion.identity);
	}

	private void MirrorProcessed()
	{
	}
}
