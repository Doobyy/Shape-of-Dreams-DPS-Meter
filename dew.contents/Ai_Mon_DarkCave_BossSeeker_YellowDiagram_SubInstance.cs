using Mirror;
using UnityEngine;

public class Ai_Mon_DarkCave_BossSeeker_YellowDiagram_SubInstance : InstantDamageInstance
{
	public GameObject fxTelegraph;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			FxPlayNetworked(fxTelegraph, position, rotation);
		}
		base.OnCreate();
	}

	private void MirrorProcessed()
	{
	}
}
