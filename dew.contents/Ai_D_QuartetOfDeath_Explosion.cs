using System;
using Mirror;

public class Ai_D_QuartetOfDeath_Explosion : AbilityInstance
{
	[NonSerialized]
	public int currentStack;

	public ScalingValue amountPerStack;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			PhysicalDamage((float)currentStack * GetValue(amountPerStack)).SetElemental(ElementalType.Dark).Dispatch(info.target);
			Destroy();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		currentStack = 0;
	}

	private void MirrorProcessed()
	{
	}
}
