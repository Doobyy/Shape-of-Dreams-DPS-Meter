using Mirror;

public class Se_C_Hemorrhage_EmpowerAttack : StatusEffect
{
	public float duration = 8f;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ResetCooldown(info.caster.Ability.attackAbility);
		ShowOnScreenTimer();
		SetTimer(duration);
		DoAttackEmpower((EventInfoAttackEffect effect, int _) =>
		{
			if (!effect.chain.DidReact(this))
			{
				CreateStatusEffect(effect.victim, (Se_C_Hemorrhage_Bleed b) =>
				{
					b.strength = effect.strength;
					b.chain = effect.chain.New(this);
				});
			}
		}, 1, DestroyIfActive);
	}

	private void MirrorProcessed()
	{
	}
}
