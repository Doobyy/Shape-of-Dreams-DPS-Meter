using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Gem_E_Twilight : Gem
{
	public GameObject consumeStackEffect;

	public float delay;

	public ScalingValue dmgPerStack;

	public ScalingValue healPerStack;

	public int minStack = 3;

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.EntityEvent_OnAttackFiredBeforePrepare += new Action<EventInfoAttackFired>(EmpowerAttack);
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)oldOwner != null)
		{
			oldOwner.EntityEvent_OnAttackFiredBeforePrepare -= new Action<EventInfoAttackFired>(EmpowerAttack);
		}
	}

	protected override void OnDealDamage(EventInfoDamage info)
	{
		base.OnDealDamage(info);
		if (owner.CheckEnemyOrNeutral(info.victim) && !info.chain.DidReact(this))
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			info.actor.LockDestroy();
			yield return new WaitForSeconds(delay);
			info.actor.UnlockDestroy();
			if (!((UnityEngine.Object)(object)info.victim == null) && !((UnityEngine.Object)(object)owner == null))
			{
				int elementalStack = info.victim.Status.GetElementalStack(ElementalType.Dark);
				if (elementalStack >= minStack)
				{
					FxPlayNewNetworked(consumeStackEffect, info.victim);
					info.actor.MagicDamage(GetValue(dmgPerStack) * (float)elementalStack).SetAttr(DamageAttribute.IsCrit).Dispatch(info.victim, info.chain.New(this));
					new HealData(GetValue(healPerStack) * (float)elementalStack).SetActor(info.actor).SetCrit().SetCanMerge()
						.Dispatch(owner, info.chain.New(this));
					if (info.victim.Status.TryGetStatusEffect<Se_Elm_Dark>(out var effect))
					{
						effect.Destroy();
					}
					NotifyUse();
				}
			}
		}
	}

	private void EmpowerAttack(EventInfoAttackFired obj)
	{
		obj.instance.dealtDamageProcessor.Add(delegate(ref DamageData data, Actor actor, Entity target)
		{
			data.SetElemental(ElementalType.Dark);
		}, -2000);
	}

	private void MirrorProcessed()
	{
	}
}
