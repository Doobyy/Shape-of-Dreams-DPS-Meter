using System;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_F_FR_ChangeToUltHitReset : StarEffect
{
	public float ultCooldownTime = 100f;

	public float durationIncreasePerKill = 1.5f;

	public float durationIncreasePerHit = 0.3f;

	public override Type heroType => typeof(Hero_Cetus);

	public override Type skillType => typeof(St_R_FrozenFists);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoSkill((SkillTrigger s) =>
		{
			s.configs[0].cooldownTime = ultCooldownTime;
			s.type = SkillType.Ultimate;
			return () =>
			{
				s.configs[0].cooldownTime = DewResources.GetByType<St_R_FrozenFists>(default(ResourceLoadSettings)).configs[0].cooldownTime;
				s.type = SkillType.Normal;
			};
		});
		hero.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ActorEventOnDealDamage);
		hero.ClientHeroEvent_OnKillOrAssist += new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
		hero.ClientEntityEvent_OnStatusEffectAdded += new Action<EventInfoStatusEffect>(ClientEntityEventOnStatusEffectAdded);
	}

	private void ClientEntityEventOnStatusEffectAdded(EventInfoStatusEffect obj)
	{
		if (obj.effect is Se_R_FrozenFists actor)
		{
			CreateBasicEffect(hero, new UnstoppableEffect(), 3600f).DestroyOnDestroy(actor);
		}
	}

	private void ClientHeroEventOnKillOrAssist(EventInfoKill obj)
	{
		ExtendDuration(durationIncreasePerKill);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)hero == null))
		{
			hero.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(ActorEventOnDealDamage);
			hero.ClientHeroEvent_OnKillOrAssist -= new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
			hero.ClientEntityEvent_OnStatusEffectAdded -= new Action<EventInfoStatusEffect>(ClientEntityEventOnStatusEffectAdded);
		}
	}

	private void ActorEventOnDealDamage(EventInfoDamage obj)
	{
		if (obj.actor is Ai_R_FrozenFists_Attack)
		{
			ExtendDuration(durationIncreasePerHit);
		}
	}

	private void ExtendDuration(float amount)
	{
		if (hero.Status.TryGetStatusEffect<Se_R_FrozenFists>(out var effect) && effect.remainingDuration.HasValue && effect.maxDuration.HasValue)
		{
			effect.SetTimer(effect.maxDuration.Value, Mathf.Min(effect.remainingDuration.Value + amount, effect.maxDuration.Value));
		}
	}

	private void MirrorProcessed()
	{
	}
}
