using Mirror;
using UnityEngine;

public class Se_Elm_Dark : ElementalStatusEffect
{
	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.takenDamageProcessor.Add(DamageAmplifier);
			victim.Status.darkStack = stack;
		}
	}

	private void DamageAmplifier(ref DamageData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this) && data.elemental == ElementalType.Dark)
		{
			if (stack >= 3)
			{
				data.SetAttr(DamageAttribute.IsCrit);
			}
			Hero hero = actor.FindFirstOfType<Hero>();
			float num = 0f;
			if ((Object)(object)hero != null)
			{
				num = hero.Status.darkEffectAmp;
			}
			float value = (float)stack * (0.05f + num);
			data.ApplyAmplification(value);
			data.SetAmountModifiedBy(this);
		}
	}

	protected override void OnStackChange(int oldStack, int newStack)
	{
		base.OnStackChange(oldStack, newStack);
		if ((newStack != 0 || !victim.Status.isDead) && ((NetworkBehaviour)this).isServer)
		{
			victim.Status.darkStack = newStack;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			victim.takenDamageProcessor.Remove(DamageAmplifier);
			if (victim.Status.isAlive)
			{
				victim.Status.darkStack = 0;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
