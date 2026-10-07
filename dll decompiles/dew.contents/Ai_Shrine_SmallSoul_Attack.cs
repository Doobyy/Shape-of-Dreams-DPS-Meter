using Mirror;
using UnityEngine;

public class Ai_Shrine_SmallSoul_Attack : AbilityInstance
{
	public ScalingValue damage;

	public GameObject fxHit;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			Damage(damage).SetActor(info.caster).SetAttr(DamageAttribute.ForceMergeNumber).Dispatch(info.target);
			FxPlayNetworked(fxHit, info.target);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
