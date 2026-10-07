using Mirror;

[SaveActor(true)]
public class PersistentStatBonusEffect : StatusEffect
{
	[SaveVar(SaveVarFlags.Default)]
	public StatBonus bonus;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			bonus = DoStatBonus(bonus);
		}
	}

	private void MirrorProcessed()
	{
	}
}
