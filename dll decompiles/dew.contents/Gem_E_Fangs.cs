using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Gem_E_Fangs : Gem
{
	public ScalingValue cooldownReductionOnCrit;

	public float searchRange;

	public GameObject fxActivate;

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.EntityEvent_OnAttackFired += new Action<EventInfoAttackFired>(CheckCritical);
			newOwner.EntityEvent_OnAttackHit += new Action<EventInfoAttackHit>(EntityEventOnAttackHit);
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Actor[] array = children.ToArray();
		foreach (Actor actor in array)
		{
			if (actor is Se_GenericEffectContainer && !actor.IsNullOrInactive())
			{
				actor.Destroy();
			}
		}
		if ((UnityEngine.Object)(object)oldOwner != null)
		{
			oldOwner.EntityEvent_OnAttackFired -= new Action<EventInfoAttackFired>(CheckCritical);
			oldOwner.EntityEvent_OnAttackHit -= new Action<EventInfoAttackHit>(EntityEventOnAttackHit);
		}
	}

	private void CheckCritical(EventInfoAttackFired obj)
	{
		if (obj.isNextAttackFourthAttack)
		{
			AttackCriticalEffect be = new AttackCriticalEffect();
			be.onUse = () =>
			{
				be.parent.DestroyIfActive();
			};
			CreateBasicEffect(owner, be, float.PositiveInfinity, "FangsCrit").DestroyOnDestroy(this);
			NotifyUse();
		}
	}

	private void EntityEventOnAttackHit(EventInfoAttackHit obj)
	{
		if (obj.isCrit && !(obj.strength < 0.999f))
		{
			if ((UnityEngine.Object)(object)skill != null)
			{
				ApplyCooldownReduction(skill, GetValue(cooldownReductionOnCrit));
			}
			FxPlayNewNetworked(fxActivate, owner);
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(0.15f);
			Shoot();
			yield return new WaitForSeconds(0.1f);
			Shoot();
		}
		void Shoot()
		{
			if (isValid)
			{
				List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, owner.agentPosition, searchRange, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
				{
					sortComparer = CollisionCheckSettings.DistanceFromCenter
				});
				if (list.Count > 0)
				{
					CreateAbilityInstance<Ai_Gem_E_Fangs_Projectile>(owner.position, null, new CastInfo(owner, list[0])).onEntity += (Action<Projectile.EntityHit>)((Projectile.EntityHit hit) =>
					{
						NotifyUse();
					});
				}
				handle.Return();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
