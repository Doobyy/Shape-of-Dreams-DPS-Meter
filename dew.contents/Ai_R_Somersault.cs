using Mirror;

public class Ai_R_Somersault : AbilityInstance
{
	public float jumpDodgeDuration = 0.6f;

	public float decaySpeedDuration = 1f;

	public float postDelay;

	public ScalingValue speedStrength;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		CreateBasicEffect(info.caster, new InvulnerableEffect(), jumpDodgeDuration);
		CreateBasicEffect(info.caster, new UncollidableEffect(), jumpDodgeDuration);
		Entity caster = info.caster;
		caster.Control.StartChannel(new Channel
		{
			blockedActions = (Channel.BlockedAction.Ability | Channel.BlockedAction.Attack | Channel.BlockedAction.Dodge),
			duration = jumpDodgeDuration + postDelay,
			onComplete = () =>
			{
				if (!caster.IsNullInactiveDeadOrKnockedOut())
				{
					CreateBasicEffect(caster, new SpeedEffect
					{
						strength = -60f,
						decay = true
					}, postDelay);
				}
			}
		});
		Se_R_Somersault_CritBonus se_R_Somersault_CritBonus = info.caster.Status.FindStatusEffect((Se_R_Somersault_CritBonus se) => se.IsDescendantOf(firstTrigger));
		if (!se_R_Somersault_CritBonus.IsNullOrInactive())
		{
			se_R_Somersault_CritBonus.Destroy();
		}
		CreateStatusEffect<Se_R_Somersault_CritBonus>(info.caster);
		CreateBasicEffect(info.caster, new SpeedEffect
		{
			decay = true,
			strength = GetValue(speedStrength)
		}, decaySpeedDuration, "SomersaultSpeed");
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
