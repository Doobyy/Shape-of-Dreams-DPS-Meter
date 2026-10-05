using Mirror;

public class Se_MirageSkin_Oblivion_Silenced : StatusEffect
{
	public float duration = 5f;

	private AbilityLockHandle _handle;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(duration);
			ShowOnScreenTimer();
			_handle = victim.Ability.GetNewAbilityLockHandle(shouldShowLockIcon: true);
			_handle.LockAllActiveMainSkillsCast();
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
