using Mirror;
using UnityEngine;

public class Se_Mon_SnowMountain_VikingWarrior_DmgReducer : StatusEffect
{
	public float dmgReduction;

	public GameObject fxBlocked;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			CreateBasicEffect(victim, new UnstoppableEffect(), float.PositiveInfinity);
			victim.takenDamageProcessor.Add(ReduceDamage);
			DestroyOnDeath(victim);
		}
	}

	private void ReduceDamage(ref DamageData data, Actor actor, Entity target)
	{
		Entity entity = actor.firstEntity;
		Vector2 lhs = ((Component)(object)victim).transform.forward.ToXY();
		if (!entity.IsNullInactiveDeadOrKnockedOut() && entity is Hero && (!(Vector2.Dot(lhs, (entity.position - victim.position).ToXY()) <= 0f) || (data.direction.HasValue && !(Vector2.Dot(lhs, (-data.direction.Value).ToXY()) <= 0f)) || (data.originPosition.HasValue && !(Vector2.Dot(lhs, (data.originPosition.Value - victim.position).ToXY()) <= 0f))) && victim.Control.ongoingChannels.Count <= 0 && !(actor is ElementalStatusEffect))
		{
			data.ApplyReduction(dmgReduction);
			FxPlayNetworked(fxBlocked, victim);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.takenDamageProcessor.Remove(ReduceDamage);
		}
	}

	private void MirrorProcessed()
	{
	}
}
