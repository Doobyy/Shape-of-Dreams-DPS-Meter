using Mirror;

public class Se_U_Burrow : StatusEffect
{
	public float burrowMaxDuration = 4f;

	public ScalingValue speedAmount = "25 5x";

	private AbilityLockHandle _handle;

	private AbilityTrigger.ChangedConfigHandle _configHandle;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoInvulnerable();
			DoUncollidable();
			DoSpeed(GetValue(speedAmount));
			_configHandle = firstTrigger.ChangeConfigTimedOnce(1, burrowMaxDuration, (EventInfoAbilityInstance _) =>
			{
				DestroyIfActive();
			});
			SetTimer(burrowMaxDuration);
			ShowOnScreenTimer();
			_handle = victim.Ability.GetNewAbilityLockHandle();
			_handle.LockAllMainSkillsEdit();
			_handle.LockAllAbilitiesCast();
			_handle.UnlockAbilityCast(firstTrigger.abilityIndex);
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		position = victim.agentPosition;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			_handle?.Stop();
			_configHandle?.Stop();
			_configHandle = null;
			if (!victim.IsNullInactiveDeadOrKnockedOut())
			{
				Emerge();
			}
		}
	}

	private void Emerge()
	{
		CreateAbilityInstance<Ai_U_Burrow_Emerge>(victim.agentPosition, null, new CastInfo(victim));
		DestroyIfActive();
	}

	private void MirrorProcessed()
	{
	}
}
