using Mirror;

public class Se_AcceleratedTime : StatusEffect
{
	public float dazeDuration = 0.75f;

	public StatBonus heroStatBonus;

	public StatBonus monsterStatbonus;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.Control.StartDaze(dazeDuration);
			if (victim is Monster)
			{
				DoStatBonus(monsterStatbonus);
			}
			else if (victim is Hero)
			{
				DoStatBonus(heroStatBonus);
			}
			else
			{
				Destroy();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
