using System;
using Mirror;
using UnityEngine;

public class St_D_Resolve : SkillTrigger
{
	public GameObject chargeEffectOnWeapon;

	public GameObject chargeEffectOnlyOwner;

	protected override void OnEquip(Entity newOwner)
	{
		base.OnEquip(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.EntityEvent_OnAttackFired += new Action<EventInfoAttackFired>(CheckFourth);
		}
	}

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			formerOwner.EntityEvent_OnAttackFired -= new Action<EventInfoAttackFired>(CheckFourth);
		}
	}

	private void CheckFourth(EventInfoAttackFired obj)
	{
		fillAmount = obj.everyFourAttackNormalizedProgress;
		if (obj.isNextAttackFourthAttack)
		{
			AttackCriticalEffect be = new AttackCriticalEffect();
			be.onUse = () =>
			{
				FxStopNetworked(chargeEffectOnWeapon);
				be.parent.DestroyIfActive();
			};
			CreateBasicEffect(owner, be, float.PositiveInfinity, "ResolveCrit");
			FxPlayNetworked(chargeEffectOnWeapon, owner);
			FxPlayNewNetworked(chargeEffectOnlyOwner, owner);
		}
		if (obj.isThisAttackFourthAttack)
		{
			Vector3 forward = (((UnityEngine.Object)(object)obj.info.target != null) ? (obj.info.target.position - owner.position).Flattened().normalized : (Quaternion.Euler(0f, obj.info.angle, 0f) * Vector3.forward));
			if (forward.sqrMagnitude < 0.001f)
			{
				forward = ((Component)(object)owner).transform.forward;
			}
			Quaternion value = Quaternion.LookRotation(forward);
			CreateAbilityInstance(owner.position, value, new CastInfo(owner, value.eulerAngles.y), (Ai_D_Resolve_Push p) =>
			{
				p.attack = obj.instance as MeleeAttackInstance;
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
