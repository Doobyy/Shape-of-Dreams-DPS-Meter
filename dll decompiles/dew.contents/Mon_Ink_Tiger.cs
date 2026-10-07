public class Mon_Ink_Tiger : Monster
{
	public class Ad_TigerHit
	{
		public float lastHitTime;
	}

	public float shadowWalkMinTime;

	public float dashMinRange;

	private Se_Mon_Ink_Tiger_Walk _tigerWalk;

	public override void OnStartServer()
	{
	}

	protected override void OnDestroyActor()
	{
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
	}

	private void MirrorProcessed()
	{
	}
}
