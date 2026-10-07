using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossMaw_ShadowWalk : AbilityInstance
{
	public float dashDis;

	public DewCollider range;

	public int spawnCount;

	public float initDelay;

	public float projectileDelay;

	public float spawnInterval;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = initDelay
		});
		CreateStatusEffect(info.caster, (Se_Mon_Special_BossMaw_ShadowWalk b) =>
		{
			b.disappearDuration = initDelay;
		});
		Vector3 normalized = (info.target.GetAIAgentPosition(info.caster) - info.caster.agentPosition).normalized;
		float num = dashDis;
		normalized = Quaternion.Euler(0f, Random.Range(30, -30), 0f) * normalized;
		Vector3 end = info.caster.agentPosition + normalized * num;
		end = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, end);
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			destination = end,
			canGoOverTerrain = false,
			duration = initDelay,
			ease = DewEase.EaseInOutQuad,
			isFriendly = true,
			isCanceledByCC = false
		});
		yield return new SI.WaitForSeconds(initDelay);
		for (int i = 0; i < spawnCount; i++)
		{
			range.transform.position = info.caster.position;
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
			{
				sortComparer = CollisionCheckSettings.Random
			});
			Vector3 vector = info.caster.section.GetAnyRandomNode() + Random.insideUnitSphere.Flattened() * 2f;
			if (entities.Count > 0)
			{
				Entity target = entities[Random.Range(0, entities.Count)];
				vector = AbilityTrigger.PredictPoint_Simple(info.caster, Random.Range(0.25f, 1f), target, projectileDelay) + Random.insideUnitSphere.Flattened() * 6f;
			}
			handle.Return();
			vector = Dew.GetPositionOnGround(vector);
			CreateAbilityInstance(info.caster.position, Quaternion.identity, new CastInfo(info.caster, vector), (Ai_Mon_Special_BossMaw_ShadowWalk_Projectile b) =>
			{
				b.delay = projectileDelay;
			});
			yield return new SI.WaitForSeconds(spawnInterval);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
