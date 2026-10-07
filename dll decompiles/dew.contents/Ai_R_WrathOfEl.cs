public class Ai_R_WrathOfEl : InstantDamageInstance
{
	public ScalingValue maxRepeatCount;

	private int _currentRepeatCount;

	private bool _didRepeat;

	public override bool reuseInRoom
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	protected override void OnDisable()
	{
	}

	protected override void OnCreate()
	{
	}

	protected override void OnHit(Entity entity)
	{
	}

	private void MirrorProcessed()
	{
	}
}
