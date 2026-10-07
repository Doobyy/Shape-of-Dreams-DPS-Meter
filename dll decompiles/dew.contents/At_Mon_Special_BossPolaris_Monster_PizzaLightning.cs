public class At_Mon_Special_BossPolaris_Monster_PizzaLightning : AbilityTrigger
{
	private ActorRef<StatusEffect> _invul;

	public override void OnCastStart(int configIndex, CastInfo info)
	{
		_invul = CreateBasicEffect(owner, new InvulnerableEffect(), 5f);
		base.OnCastStart(configIndex, info);
	}

	protected override void OnCastCancel(int configIndex, CastInfo info)
	{
		if (!_invul.IsNullOrInactive())
		{
			_invul.Get().Destroy();
		}
		_invul = null;
		base.OnCastCancel(configIndex, info);
	}

	public override AbilityInstance OnCastComplete(int configIndex, CastInfo info)
	{
		if (!_invul.IsNullOrInactive())
		{
			_invul.Get().SetTimer(1f);
		}
		_invul = null;
		return base.OnCastComplete(configIndex, info);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_invul = null;
	}

	private void MirrorProcessed()
	{
	}
}
