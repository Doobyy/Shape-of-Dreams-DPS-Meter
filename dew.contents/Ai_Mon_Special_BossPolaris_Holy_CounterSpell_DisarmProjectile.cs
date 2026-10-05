using System;
using UnityEngine;

public class Ai_Mon_Special_BossPolaris_Holy_CounterSpell_DisarmProjectile : StandardProjectile
{
	public float stunDuration = 1f;

	[NonSerialized]
	public SkillTrigger targetSkill;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Entity entity = hit.entity;
		Hero h = entity as Hero;
		if (h == null)
		{
			return;
		}
		CreateBasicEffect(hit.entity, new StunEffect(), stunDuration);
		if (targetSkill.IsNullOrInactive() || (UnityEngine.Object)(object)targetSkill.owner != (UnityEngine.Object)(object)h)
		{
			return;
		}
		h.Skill.UnequipSkill(targetSkill.skillType, default);
		if ((UnityEngine.Object)(object)targetSkill.owner != null)
		{
			return;
		}
		targetSkill.RpcSetPositionAndRotation(new Vector3(-999f, -999f, -999f), targetSkill.rotation);
		Vector3 destination = default;
		for (int i = 0; i < 30; i++)
		{
			destination = h.agentPosition + UnityEngine.Random.onUnitSphere.Flattened().normalized * 16f;
			destination = Dew.GetValidAgentDestination_LinearSweep(h.agentPosition, destination);
			if (Vector3.Distance(destination, h.agentPosition) > 12f)
			{
				break;
			}
		}
		CreateAbilityInstance(h.agentPosition, null, new CastInfo(info.caster, destination), (Ai_Mon_Special_BossPolaris_Holy_CounterSpell_SkillProjectile ai) =>
		{
			ai.SetCustomStartPosition(h.Visual.GetCenterPosition());
			ai.tintColor = Dew.GetRarityColor(targetSkill.rarity);
		}).ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
		{
			targetSkill.RpcSetPositionAndRotation(destination, targetSkill.rotation);
		});
	}

	private void MirrorProcessed()
	{
	}
}
