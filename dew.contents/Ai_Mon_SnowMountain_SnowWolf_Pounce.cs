public class Ai_Mon_SnowMountain_SnowWolf_Pounce : DashAttackInstance
{
	private float _baseDashDistance;

	private bool _cachedBaseDashDistance;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		if (!_cachedBaseDashDistance)
		{
			_baseDashDistance = dash.distance;
			_cachedBaseDashDistance = true;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		dash.distance = _baseDashDistance;
	}

	private void MirrorProcessed()
	{
	}
}
