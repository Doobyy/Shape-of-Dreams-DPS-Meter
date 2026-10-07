using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_D_DoubleTap : StackedStatusEffect
{
	public GameObject chargeEffect;

	public GameObject firstShotEffect;

	public GameObject secondShotEffect;

	public ScalingValue critChanceBonus;

	public ScalingValue attackDamageBonus;

	public bool allowMovementSkill;

	public float shootDelay;

	private StatBonus _bonus;

	public Hero heroVictim => victim as Hero;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			heroVictim.EntityEvent_OnAttackFired += new Action<EventInfoAttackFired>(EntityEventOnAttackFired);
			heroVictim.ClientHeroEvent_OnSkillUse += new Action<EventInfoSkillUse>(HeroEventOnSkillUse);
			_bonus = new StatBonus
			{
				critChanceFlat = GetValue(critChanceBonus),
				attackDamageFlat = GetValue(attackDamageBonus)
			};
			heroVictim.Status.AddStatBonus(_bonus);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)heroVictim == null))
		{
			victim.EntityEvent_OnAttackFired -= new Action<EventInfoAttackFired>(EntityEventOnAttackFired);
			heroVictim.ClientHeroEvent_OnSkillUse -= new Action<EventInfoSkillUse>(HeroEventOnSkillUse);
			if ((UnityEngine.Object)(object)firstTrigger != null)
			{
				firstTrigger.fillAmount = 0f;
			}
			if (_bonus != null)
			{
				heroVictim.Status.RemoveStatBonus(_bonus);
			}
		}
	}

	private void HeroEventOnSkillUse(EventInfoSkillUse obj)
	{
		if ((obj.type == HeroSkillLocation.Q || obj.type == HeroSkillLocation.W || obj.type == HeroSkillLocation.E || obj.type == HeroSkillLocation.R || (allowMovementSkill && obj.type == HeroSkillLocation.Movement)) && stack < maxStack)
		{
			AddStack();
			FxPlayNetworked(chargeEffect, victim);
			ResetCooldown(victim.Ability.attackAbility);
		}
	}

	private void EntityEventOnAttackFired(EventInfoAttackFired obj)
	{
		if (stack > 0 && !(Vector3.Magnitude(obj.instance.info.point - At_Atk_LacertaRifle.MagicNumber) < 0.01f))
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			RemoveStack();
			FxPlayNewNetworked(firstShotEffect, victim);
			yield return new WaitForSeconds(shootDelay * victim.Ability.attackAbility.GetCooldownTimeMultiplier(victim.Ability.attackAbility.currentConfigIndex));
			if (victim.Ability.attackAbility is At_Atk_LacertaRifle at_Atk_LacertaRifle)
			{
				Entity entity = obj.info.target;
				if (entity.IsNullInactiveDeadOrKnockedOut() || entity.currentHealth + entity.Status.currentShield < victim.Status.attackDamage)
				{
					Vector3 pivot = (((UnityEngine.Object)(object)entity == null) ? victim.owner.cursorWorldPos : entity.position);
					float radius = victim.Ability.attackAbility.currentConfig.effectiveRange + 1.5f;
					List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, victim.agentPosition, radius, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
					{
						sortComparer = Comparer<Entity>.Create((Entity x, Entity y) => Vector2.Distance(pivot.ToXY(), x.agentPosition.ToXY()).CompareTo(Vector2.Distance(pivot.ToXY(), y.agentPosition.ToXY())))
					});
					if (list.Count > 0)
					{
						entity = list[0];
					}
					handle.Return();
				}
				ResetCooldown(at_Atk_LacertaRifle);
				if ((UnityEngine.Object)(object)entity != null)
				{
					at_Atk_LacertaRifle.Shoot(entity);
					FxPlayNewNetworked(secondShotEffect, victim);
				}
				else
				{
					at_Atk_LacertaRifle.Shoot((obj.info.angle != 0f) ? obj.info.angle : CastInfo.GetAngle(victim.owner.cursorWorldPos - victim.agentPosition));
				}
			}
		}
	}

	protected override void OnStackChange(int oldStack, int newStack)
	{
		base.OnStackChange(oldStack, newStack);
		if (((NetworkBehaviour)this).isServer)
		{
			firstTrigger.fillAmount = (float)newStack / (float)maxStack;
		}
	}

	private void MirrorProcessed()
	{
	}
}
