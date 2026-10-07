using System;
using Mirror;
using UnityEngine;

public class Se_U_EternalFlame_Curse : StatusEffect
{
	public ScalingValue duration;

	public GameObject fxAddFireStack;

	private Action<EventInfoDamage> _onTakeDamageCached;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(GetValue(duration));
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if ((!((UnityEngine.Object)(object)obj.actor != (UnityEngine.Object)(object)info.caster) || obj.actor.IsDescendantOf(info.caster)) && !(UnityEngine.Random.value > obj.damage.procCoefficient))
		{
			ApplyElemental(ElementalType.Fire, victim);
			FxPlayNetworked(fxAddFireStack, victim);
		}
	}

	private void MirrorProcessed()
	{
	}
}
