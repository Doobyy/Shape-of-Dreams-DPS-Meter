using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_L_Multishot_Buff : StatusEffect
{
	public ScalingValue hasteAmount;

	public ScalingValue speedAmount;

	public DewCollider subTargetRange;

	public int arrowCount = 3;

	public float firstArrowDelay = 0.1f;

	public float arrowInterval = 0.075f;

	public float duration = 6f;

	public bool onlyFirstArrowAttackEffect;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoHaste(GetValue(hasteAmount));
			DoSpeed(GetValue(speedAmount));
			victim.EntityEvent_OnAttackEffectTriggered += new Action<EventInfoAttackEffect>(EntityEventOnAttackEffectTriggered);
			SetTimer(duration);
			ShowOnScreenTimer();
			ResetCooldown(info.caster.Ability.attackAbility);
		}
	}

	private void EntityEventOnAttackEffectTriggered(EventInfoAttackEffect obj)
	{
		StartSequence(ShootArrowSequence(obj));
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)victim == null))
		{
			victim.EntityEvent_OnAttackEffectTriggered -= new Action<EventInfoAttackEffect>(EntityEventOnAttackEffectTriggered);
		}
	}

	private IEnumerator ShootArrowSequence(EventInfoAttackEffect obj)
	{
		if (obj.chain.DidReact(this, checkOnlyType: true))
		{
			yield break;
		}
		Entity mainTarget = obj.victim;
		yield return new SI.WaitForSeconds(firstArrowDelay);
		for (int i = 0; i < arrowCount; i++)
		{
			if ((UnityEngine.Object)(object)mainTarget == null)
			{
				break;
			}
			subTargetRange.transform.position = mainTarget.position;
			List<Entity> entities = subTargetRange.GetEntities(out var handle, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
			{
				sortComparer = CollisionCheckSettings.Random
			});
			Entity entity = mainTarget;
			if (entities.Count > 0)
			{
				entity = entities[0];
			}
			handle.Return();
			if ((UnityEngine.Object)(object)entity == null)
			{
				break;
			}
			bool triggersAttackEffect = !onlyFirstArrowAttackEffect || i == 0;
			CreateAbilityInstance(info.caster.position, Quaternion.identity, new CastInfo(info.caster, entity), (Ai_L_Multishot_Arrow a) =>
			{
				a.strength = obj.strength;
				a.triggerAttackEffect = triggersAttackEffect;
				a.chain = obj.chain.New(this);
			});
			yield return new SI.WaitForSeconds(arrowInterval);
		}
	}

	private void MirrorProcessed()
	{
	}
}
