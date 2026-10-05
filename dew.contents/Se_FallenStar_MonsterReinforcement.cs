using System;
using Mirror;
using UnityEngine;

public class Se_FallenStar_MonsterReinforcement : StatusEffect
{
	[NonSerialized]
	public float bonusHealthPercentage;

	[NonSerialized]
	public float bonusPowerPercentage;

	private GameObject _originalFxDeath;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		_originalFxDeath = victim.Visual.model.fxDeath;
		if (_originalFxDeath != null)
		{
			victim.Visual.model.fxDeath = null;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				maxHealthPercentage = bonusHealthPercentage,
				attackDamagePercentage = bonusPowerPercentage,
				abilityPowerPercentage = bonusPowerPercentage
			});
			victim.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(OnDealDamage);
			victim.EntityEvent_OnDeath += new Action<EventInfoKill>(EntityEventOnDeath);
		}
	}

	private void EntityEventOnDeath(EventInfoKill obj)
	{
		victim.Visual.DisableRenderers();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_originalFxDeath != null)
		{
			if ((UnityEngine.Object)(object)victim != null && victim.Visual.model != null && victim.Visual.model.fxDeath == null)
			{
				victim.Visual.model.fxDeath = _originalFxDeath;
			}
			_originalFxDeath = null;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(OnDealDamage);
				victim.EntityEvent_OnDeath -= new Action<EventInfoKill>(EntityEventOnDeath);
			}
			FxStopNetworked(startEffect);
		}
	}

	private void OnDealDamage(EventInfoDamage obj)
	{
		if (!((UnityEngine.Object)(object)obj.victim == (UnityEngine.Object)(object)victim) && obj.victim is Hero hero && !hero.Status.HasStatusEffect<Se_FallenStar_WaveringIdentity>())
		{
			CreateStatusEffect<Se_FallenStar_WaveringIdentity>(hero, new CastInfo(victim));
		}
	}

	private void MirrorProcessed()
	{
	}
}
