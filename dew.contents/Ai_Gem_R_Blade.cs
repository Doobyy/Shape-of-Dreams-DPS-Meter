using Mirror;
using UnityEngine;

public class Ai_Gem_R_Blade : AbilityInstance
{
	public ScalingValue dmgFactor;

	public float procCoefficient;

	public GameObject slashEffect;

	public GameObject hitEffect;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && !info.target.IsNullInactiveDeadOrKnockedOut())
		{
			FxPlayNetworked(slashEffect, info.target);
			Damage(dmgFactor, procCoefficient).Dispatch(info.target, chain);
			FxPlayNetworked(hitEffect, info.target);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
