using System.Collections.Generic;
using System.Linq;
using Mirror;

public class Se_GravityTraining : StatusEffect
{
	private AbilityLockHandle _handle;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (victim is Hero)
		{
			DoSlow(40f);
			return;
		}
		DoSlow(25f);
		victim.Control.StartDaze(0.35f);
		_handle = victim.Ability.GetNewAbilityLockHandle();
		KeyValuePair<int, AbilityTrigger>[] array = victim.Ability.abilities.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<int, AbilityTrigger> keyValuePair = array[i];
			if (keyValuePair.Value.currentConfig.selfValidator.isMovementAbility)
			{
				_handle.LockAbilityCast(keyValuePair.Key);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _handle != null)
		{
			_handle.UnlockAllAbilitiesCast();
			_handle.Stop();
			_handle = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
