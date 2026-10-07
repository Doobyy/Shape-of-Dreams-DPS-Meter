public class Ai_R_UnbreakableDetermination_SubExplosion : InstantDamageInstance, ACH_THEYRE_JUST_BIG_CATS.ILightingActor
{
	private ScalingValue _originalDmgFactor;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_originalDmgFactor = dmgFactor;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		dmgFactor = _originalDmgFactor;
	}

	private void MirrorProcessed()
	{
	}
}
