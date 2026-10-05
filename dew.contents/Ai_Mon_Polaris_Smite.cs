using Mirror;
using UnityEngine;

public class Ai_Mon_Polaris_Smite : AbilityInstance
{
	public ScalingValue damage;

	public GameObject fxHit;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if ((Object)(object)info.target == null)
		{
			if (((NetworkBehaviour)this).isServer)
			{
				Destroy();
			}
			return;
		}
		((Component)(object)this).transform.position = info.target.position;
		if (((NetworkBehaviour)this).isServer)
		{
			FxPlayNewNetworked(fxHit, info.target);
			CreateBasicEffect(info.target, new StunEffect(), 3f, "PolarisSmiteStun", DuplicateEffectBehavior.UsePrevious);
			float num = GetValue(damage);
			if (info.target is Monster)
			{
				num *= 7f;
			}
			DefaultDamage(num).SetElemental(ElementalType.Light).Dispatch(info.target);
		}
	}

	private void MirrorProcessed()
	{
	}
}
