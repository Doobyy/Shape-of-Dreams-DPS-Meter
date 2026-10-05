using Mirror;
using UnityEngine;

public class Ai_Mon_Despair_BossAzurak_StompBlock_Artillery : StandardProjectile
{
	public GameObject fxTelegraph;

	public override bool reuseInRoom => true;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		SetCustomStartPosition(info.caster.position + Vector3.up * 3f);
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		FxPlayNew(fxTelegraph, info.point, Quaternion.identity);
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
		}
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		CreateAbilityInstance<Ai_Mon_Despair_BossAzurak_StompBlock_Artillery_Damage>(position, null, new CastInfo(info.caster));
	}

	private void MirrorProcessed()
	{
	}
}
