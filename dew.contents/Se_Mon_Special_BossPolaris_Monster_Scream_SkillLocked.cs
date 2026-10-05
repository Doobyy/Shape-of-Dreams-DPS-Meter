using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Mon_Special_BossPolaris_Monster_Scream_SkillLocked : StatusEffect
{
	public float duration = 5f;

	public float crippleAmount = 30f;

	private AbilityLockHandle _handle;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		CreateBasicEffect(victim, new StunEffect(), 1f);
		_handle = victim.Ability.GetNewAbilityLockHandle(shouldShowLockIcon: true);
		if (victim is Hero hero)
		{
			if (!hero.Skill.Movement.IsNullOrInactive())
			{
				ApplyCooldownReductionByRatio(hero.Skill.Movement, 1f, ignoreCanReceiveCooldown: true);
			}
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
			if (list.Count != 0)
			{
				list.Shuffle();
				for (int i = 0; i < 2 && i < list.Count; i++)
				{
					int index = list[i];
					_handle.LockAbilityCast(index);
					_handle.LockAbilityEdit(index);
				}
			}
		}
		DoCripple(crippleAmount);
		SetTimer(duration);
		ShowOnScreenTimer(null, new Color(0.9f, 0.2f, 0.2f, 1f));
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
