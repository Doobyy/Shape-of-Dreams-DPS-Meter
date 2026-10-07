using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Curse_FaintMemory : CurseStatusEffect
{
	public GameObject fxNewLock;

	public GameObject fxLockRemoved;

	public float refreshTime;

	public float[] sealDuration;

	private List<AbilityLockHandle> _handles = new List<AbilityLockHandle>();

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		Hero hero = (Hero)victim;
		List<HeroSkillLocation> pool = new List<HeroSkillLocation>();
		HeroSkillLocation lastLocked = HeroSkillLocation.Identity;
		while (true)
		{
			pool.Clear();
			if ((Object)(object)hero.Skill.Q != null)
			{
				pool.Add(HeroSkillLocation.Q);
			}
			if ((Object)(object)hero.Skill.W != null)
			{
				pool.Add(HeroSkillLocation.W);
			}
			if ((Object)(object)hero.Skill.E != null)
			{
				pool.Add(HeroSkillLocation.E);
			}
			if ((Object)(object)hero.Skill.R != null)
			{
				pool.Add(HeroSkillLocation.R);
			}
			pool.Remove(lastLocked);
			AbilityLockHandle handle;
			if (pool.Count > 0)
			{
				handle = hero.Ability.GetNewAbilityLockHandle(shouldShowLockIcon: true);
				_handles.Add(handle);
				lastLocked = pool[Random.Range(0, pool.Count)];
				handle.LockAbilityCast((int)lastLocked);
				handle.LockAbilityEdit((int)lastLocked);
				FxPlayNetworked(fxNewLock, victim);
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
			}
			else
			{
				lastLocked = HeroSkillLocation.Identity;
			}
			yield return new SI.WaitForSeconds(refreshTime);
			continue;
			IEnumerator Routine()
			{
				yield return new WaitForSeconds(GetValue(sealDuration));
				handle.Stop();
				_handles.Remove(handle);
				FxPlayNetworked(fxLockRemoved, victim);
			}
			IEnumerator Routine()
			{
				yield return new WaitForSeconds(GetValue(sealDuration));
				handle.Stop();
				_handles.Remove(handle);
				FxPlayNetworked(fxLockRemoved, victim);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		foreach (AbilityLockHandle handle in _handles)
		{
			handle.Stop();
		}
		_handles.Clear();
	}

	private void MirrorProcessed()
	{
	}
}
