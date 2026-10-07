using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_FallenStar_WaveringIdentity : StatusEffect
{
	public float duration = 3.5f;

	public int lockCount = 2;

	private AbilityLockHandle _handle;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (victim is Hero hero)
		{
			_handle = victim.Ability.GetNewAbilityLockHandle(shouldShowLockIcon: true);
			List<int> list = new List<int>();
			if (!hero.Skill.Q.IsNullOrInactive())
			{
				list.Add(0);
			}
			if (!hero.Skill.W.IsNullOrInactive())
			{
				list.Add(1);
			}
			if (!hero.Skill.E.IsNullOrInactive())
			{
				list.Add(2);
			}
			if (!hero.Skill.R.IsNullOrInactive())
			{
				list.Add(3);
			}
			if (list.Count == 0)
			{
				list.Add(0);
				list.Add(1);
				list.Add(2);
				list.Add(3);
			}
			list.Shuffle();
			for (int i = 0; i < lockCount && i < list.Count; i++)
			{
				_handle.LockAbilityCast(list[i]);
				_handle.LockAbilityEdit(list[i]);
			}
		}
		SetTimer(duration);
		ShowOnScreenTimer("Se_FallenStar_WaveringIdentity", new Color(0.9f, 0.6f, 0f, 1f));
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _handle != null)
		{
			_handle.Stop();
		}
	}

	private void MirrorProcessed()
	{
	}
}
