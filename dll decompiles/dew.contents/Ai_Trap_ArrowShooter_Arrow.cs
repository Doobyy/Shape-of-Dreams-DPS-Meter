public class Ai_Trap_ArrowShooter_Arrow : StandardProjectile
{
	public float startHeight;

	public float dmgMaxHpRatio;

	public float monsterDmgMultiplier;

	public Knockback knockback;

	public float stunDuration;

	public override bool reuseInRoom
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	protected override void OnPrepare()
	{
	}

	protected override void OnEntity(EntityHit hit)
	{
	}

	private void MirrorProcessed()
	{
	}
}
