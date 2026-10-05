public class Ai_D_RoaringFist : InstantDamageInstance
{
	public float stunDuration;

	public DewAnimationClip animRoaringFist;

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

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
	}

	protected override void OnHit(Entity entity)
	{
	}

	private void MirrorProcessed()
	{
	}
}
