using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Star_Yubar_F_SN_Pull : AbilityInstance
{
	public DewCollider range;

	public float delay = 0.75f;

	public DewEase ease;

	public GameObject fxPull;

	protected override IEnumerator OnCreateSequenced()
	{
		yield return base.OnCreateSequenced();
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		Vector3 center = info.point;
		((Component)(object)this).transform.position = center;
		yield return new SI.WaitForSeconds(delay);
		if ((bool)fxPull)
		{
			FxPlayNetworked(fxPull, center, null);
		}
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			if (!entity.Status.hasCrowdControlImmunity && !entity.Status.hasUnstoppable)
			{
				entity.Visual.KnockUp(KnockUpStrength.Normal, isFriendly: false);
				entity.Control.StartDisplacement(new DispByDestination
				{
					affectedByMovementSpeed = false,
					canGoOverTerrain = false,
					destination = Dew.GetValidAgentDestination_Closest(entity.agentPosition, center + Random.insideUnitCircle.ToXZ() * 0.5f),
					ease = ease,
					isCanceledByCC = false,
					isFriendly = false,
					duration = 0.75f
				});
			}
		}
		handle.Return();
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
