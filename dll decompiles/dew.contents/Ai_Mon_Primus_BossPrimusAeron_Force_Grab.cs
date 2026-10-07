using Mirror;
using UnityEngine;

public class Ai_Mon_Primus_BossPrimusAeron_Force_Grab : AbilityInstance
{
	public DewCollider range;

	public GameObject fxHit;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			FxPlayNewNetworked(fxHit, entity);
			float num = Random.Range(2f, 4f);
			float duration = Random.Range(0.5f, 1f);
			Vector3 vector;
			if (Vector2.Distance(info.caster.agentPosition.ToXY(), entity.agentPosition.ToXY()) < num)
			{
				vector = info.caster.agentPosition + Random.insideUnitSphere * 1f;
				vector = Dew.GetPositionOnGround(vector);
				vector = Dew.GetValidAgentDestination_LinearSweep(entity.agentPosition, vector);
			}
			else
			{
				Vector3 normalized = (entity.agentPosition - info.caster.agentPosition).normalized;
				vector = info.caster.agentPosition + normalized * num;
				vector = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, vector);
			}
			entity.Control.StartDisplacement(new DispByDestination
			{
				destination = vector,
				ease = DewEase.EaseOutQuad,
				duration = duration,
				affectedByMovementSpeed = false,
				canGoOverTerrain = true,
				isCanceledByCC = false,
				isFriendly = false
			});
			CreateBasicEffect(entity, new SlowEffect
			{
				strength = 100f,
				decay = true
			}, 1.5f);
		}
		handle.Return();
		ResetCooldown(info.caster.Ability.attackAbility);
		ResetCooldown(info.caster.Ability.GetAbility<At_Mon_Primus_BossPrimusAeron_Force_Swipe>());
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
