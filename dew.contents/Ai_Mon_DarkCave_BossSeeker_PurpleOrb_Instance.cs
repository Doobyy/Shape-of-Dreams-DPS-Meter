using Mirror;
using UnityEngine;

public class Ai_Mon_DarkCave_BossSeeker_PurpleOrb_Instance : InstantDamageInstance
{
	public GameObject fxTelegraph;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			FxPlayNetworked(fxTelegraph, info.point, null);
		}
		base.OnCreate();
	}

	private void MirrorProcessed()
	{
	}
}
