public class At_Mon_Special_BossErebos_Ripple : AbilityTrigger
{
	private int _baseMaxCharges;

	private int _baseAddedCharges;

	private bool _cachedBaseConfig;

	protected override void Awake()
	{
		base.Awake();
		if (!_cachedBaseConfig && configs != null && configs.Length != 0)
		{
			_baseMaxCharges = configs[0].maxCharges;
			_baseAddedCharges = configs[0].addedCharges;
			_cachedBaseConfig = true;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (_cachedBaseConfig && configs != null && configs.Length != 0)
		{
			configs[0].maxCharges = _baseMaxCharges;
			configs[0].addedCharges = _baseAddedCharges;
		}
	}

	private void MirrorProcessed()
	{
	}
}
