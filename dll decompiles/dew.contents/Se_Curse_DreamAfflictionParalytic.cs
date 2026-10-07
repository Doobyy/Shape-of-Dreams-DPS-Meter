using Mirror;

public class Se_Curse_DreamAfflictionParalytic : CurseStatusEffect
{
	private AbilityLockHandle _handle;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_handle = victim.Ability.GetNewAbilityLockHandle(shouldShowLockIcon: true);
			_handle.LockMovementSkillCast();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _handle != null)
		{
			_handle.Stop();
			_handle = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
