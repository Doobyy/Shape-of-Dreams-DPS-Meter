using Mirror;
using UnityEngine;

public class Se_HeroBleedingOut : StatusEffect
{
	private class Ad_BleedOuts
	{
		public int count;
	}

	private AbilityLockHandle _handle;

	public Hero heroVictim => victim as Hero;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_handle = heroVictim.Ability.GetNewAbilityLockHandle();
			_handle.LockAllAbilitiesCast();
			DoSlow(75f);
			DoUncollidable();
			DoUntargetable();
			DoInvisible(ignoreReveal: true);
			heroVictim.takenHealProcessor.Add(Processor);
			if (!heroVictim.TryGetData<Ad_BleedOuts>(out var data))
			{
				data = new Ad_BleedOuts();
				heroVictim.AddData(data);
			}
			data.count++;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((Object)(object)heroVictim != null)
			{
				heroVictim.takenHealProcessor.Remove(Processor);
			}
			if (_handle != null)
			{
				_handle.Stop();
				_handle = null;
			}
		}
	}

	private void Processor(ref HealData data, Actor actor, Entity target)
	{
		data.ApplyRawMultiplier(0f);
	}

	private void MirrorProcessed()
	{
	}
}
