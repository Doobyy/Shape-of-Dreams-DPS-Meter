using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem_R_Scorched : Gem
{
	public ScalingValue maxCount;

	public float radius = 12f;

	public float randomMag;

	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		NotifyUse();
		((MonoBehaviour)(object)this).StartCoroutine(ScorchedRoutine(info.instance));
	}

	private IEnumerator ScorchedRoutine(AbilityInstance source)
	{
		int num = Mathf.RoundToInt(GetValue(maxCount));
		int clampedCount = Mathf.Min(num, (DewPlayer.gamePlayers.Count > 1) ? 15 : 40);
		float strengthMultiplier = (float)num / (float)clampedCount;
		source.LockDestroyFor((float)clampedCount * 0.085f + 0.5f);
		int noTargetCount = 0;
		for (int i = 0; i < clampedCount; i++)
		{
			if (!isValid)
			{
				break;
			}
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, owner.agentPosition, radius, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
			{
				includeUncollidable = true
			});
			if (list.Count > 0)
			{
				Entity target = list[Random.Range(0, list.Count)];
				CreateAbilityInstanceWithSource(source, Dew.GetPositionOnGround(AbilityTrigger.PredictPoint_Simple(owner, Random.value, target, 0.35f) + Random.insideUnitSphere.Flattened() * (randomMag * Mathf.Clamp((float)i / 5f, 0.5f, 1f))), Quaternion.identity, new CastInfo(owner), (Ai_Gem_R_Scorched_Meteor ai) =>
				{
					ai.strengthMultiplier = strengthMultiplier;
				});
				yield return new SI.WaitForSeconds(0.05f);
			}
			else
			{
				noTargetCount++;
				if (noTargetCount >= 10)
				{
					break;
				}
			}
			handle.Return();
			NotifyUse();
			yield return new SI.WaitForSeconds(0.035f);
			handle = default;
		}
	}

	private void MirrorProcessed()
	{
	}
}
