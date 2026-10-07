using Mirror;

public class Se_D_ScarOfTheWind_NextAtkDash : StatusEffect
{
	public float duration = 4f;

	private int _generation;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_generation++;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		int gen = _generation;
		DoAttackOverride<At_D_ScarOfTheWind_DashAtk>(() =>
		{
			Dew.CallDelayed(() =>
			{
				if (gen == _generation)
				{
					DestroyIfActive();
				}
			});
		});
		SetTimer(duration);
	}

	private void MirrorProcessed()
	{
	}
}
