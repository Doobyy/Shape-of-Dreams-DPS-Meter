using Mirror;
using UnityEngine;

public class Ai_Mon_Despair_BossAzurak_StompProjectile_FirstAtk : InstantDamageInstance
{
	public GameObject fxTelegraph;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			FxPlayNetworked(fxTelegraph, info.point, Quaternion.identity);
		}
		base.OnCreate();
	}

	protected override void OnAfterDelay()
	{
		base.OnAfterDelay();
		CreateAbilityInstance<Ai_Mon_Despair_BossAzurak_StompProjectile>(position, rotation, info);
	}

	private void MirrorProcessed()
	{
	}
}
