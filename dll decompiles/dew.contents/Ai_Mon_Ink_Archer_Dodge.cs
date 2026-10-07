using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_Archer_Dodge : AbilityInstance
{
	public ScalingValue dmgFactor;

	public DewCollider range;

	public Knockback knockback;

	public float duration;

	public float distance;

	public DewEase ease;

	public GameObject fxHit;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		info.caster.Control.StartDaze(duration);
		Vector3 end = info.caster.agentPosition + -info.forward * distance;
		end = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, end);
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetDirection(info.forward).SetOriginPosition(info.caster.agentPosition).Dispatch(entity);
			knockback.ApplyWithDirection(info.forward, entity);
			FxPlayNewNetworked(fxHit, entity);
		}
		handle.Return();
		ActorRef<Actor> selfRef = this;
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			affectedByMovementSpeed = false,
			canGoOverTerrain = false,
			destination = end,
			duration = duration,
			ease = ease,
			isCanceledByCC = true,
			isFriendly = true,
			onCancel = () =>
			{
				if ((Object)(object)selfRef.Get() != null)
				{
					DestroyIfActive();
				}
			},
			onFinish = () =>
			{
				if ((Object)(object)selfRef.Get() != null)
				{
					DestroyIfActive();
				}
			},
			rotateForward = false
		});
	}

	private void MirrorProcessed()
	{
	}
}
