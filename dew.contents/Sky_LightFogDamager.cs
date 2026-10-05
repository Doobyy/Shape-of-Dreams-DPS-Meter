using Mirror;
using UnityEngine;

public class Sky_LightFogDamager : Actor
{
	private class Ad_LightFog
	{
		public float lastDamageTime;
	}

	public static Sky_LightFogDamager instance;

	public bool doDamage;

	public GameObject hitEffect;

	public float tickInterval;

	public float damageInterval;

	public float fogOpacityThreshold;

	public float damageHpRatio;

	private float _lastTickTime;

	public override void OnStartServer()
	{
		base.OnStartServer();
		instance = this;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || !doDamage || Time.time - _lastTickTime < tickInterval)
		{
			return;
		}
		_lastTickTime = Time.time;
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			if (allHero.Visual.isSpawning || allHero.Control.isDisplacing || allHero.IsNullInactiveDeadOrKnockedOut() || allHero.Status.hasUncollidable || allHero.Status.hasInvulnerable)
			{
				continue;
			}
			Ad_LightFog ad_LightFog = allHero.GetData<Ad_LightFog>();
			if (!(SingletonBehaviour<Sky_LightFog>.instance.SampleFogOpacity(allHero.agentPosition) < fogOpacityThreshold) && (ad_LightFog == null || !(Time.time - ad_LightFog.lastDamageTime < damageInterval)))
			{
				if (ad_LightFog == null)
				{
					ad_LightFog = new Ad_LightFog();
					allHero.AddData(ad_LightFog);
				}
				ad_LightFog.lastDamageTime = Time.time;
				DefaultDamage(damageHpRatio * allHero.maxHealth).SetElemental(ElementalType.Light).Dispatch(allHero);
				FxPlayNewNetworked(hitEffect, allHero);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
