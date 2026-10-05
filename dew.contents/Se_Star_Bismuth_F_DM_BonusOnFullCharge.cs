using System;
using Mirror;
using UnityEngine;

public class Se_Star_Bismuth_F_DM_BonusOnFullCharge : StarEffect
{
	public int bonusCount = 2;

	public GameObject fxFullChargeShoot;

	public float cooldownPenalty = 1f;

	private Ai_QR_DistortedMind_Spawner _subscribedSpawner;

	private Action<int> _onShoot;

	public override Type heroType => typeof(Hero_Bismuth);

	public override Type skillType => typeof(St_QR_DistortedMind);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			DoSkillBonusAll(new SkillBonus
			{
				cooldownOffset = cooldownPenalty
			});
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if ((UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
		if ((UnityEngine.Object)(object)_subscribedSpawner != null && _onShoot != null)
		{
			_subscribedSpawner.onShoot -= _onShoot;
		}
		_subscribedSpawner = null;
		_onShoot = null;
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		AbilityInstance instance = obj.instance;
		Ai_QR_DistortedMind_Spawner spawner = instance as Ai_QR_DistortedMind_Spawner;
		if (spawner == null)
		{
			return;
		}
		if ((UnityEngine.Object)(object)_subscribedSpawner != null && _onShoot != null)
		{
			_subscribedSpawner.onShoot -= _onShoot;
		}
		spawner.bonusOnFullCharge = bonusCount;
		_onShoot = (int count) =>
		{
			if (count > spawner.clampedCount)
			{
				FxPlayNetworked(fxFullChargeShoot, hero);
			}
		};
		_subscribedSpawner = spawner;
		spawner.onShoot += _onShoot;
	}

	private void MirrorProcessed()
	{
	}
}
