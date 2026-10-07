public class Se_R_ShadowWalk_EmpowerAttacks : StatusEffect
{
	public ScalingValue hasteAmount;

	public float duration;

	public override bool reuseInRoom
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	protected override void OnCreate()
	{
	}

	private void EntityEventOnAttackEffectTriggered(EventInfoAttackEffect obj)
	{
	}

	protected override void OnDestroyActor()
	{
	}

	private void MirrorProcessed()
	{
	}
}
