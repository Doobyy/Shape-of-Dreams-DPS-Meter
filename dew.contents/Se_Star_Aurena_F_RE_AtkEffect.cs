using System;
using Mirror;
using UnityEngine;

public class Se_Star_Aurena_F_RE_AtkEffect : StarEffect
{
	public override Type heroType => typeof(Hero_Aurena);

	public override Type skillType => typeof(St_Q_Reduction);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		AbilityInstance instance = obj.instance;
		Ai_Q_Reduction_Projectile ai = instance as Ai_Q_Reduction_Projectile;
		if (ai == null)
		{
			return;
		}
		RefValue<bool> didEffect = new RefValue<bool>(v: false);
		ai.dealtDamageProcessor.Add(delegate(ref DamageData data, Actor actor, Entity target)
		{
			if (!didEffect && !((UnityEngine.Object)(object)actor != (UnityEngine.Object)(object)ai) && hero.CheckEnemyOrNeutral(target))
			{
				data.DoAttackEffect(AttackEffectType.Others);
				didEffect.value = true;
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
