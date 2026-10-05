public class Ai_Mon_Despair_BossAzurak_StompTargetArea : InstantDamageInstance
{
	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		position = info.point;
		base.OnCreate();
	}

	private void MirrorProcessed()
	{
	}
}
