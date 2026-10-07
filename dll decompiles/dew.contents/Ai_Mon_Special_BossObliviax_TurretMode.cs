using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossObliviax_TurretMode : AbilityInstance
{
	public float radius;

	public int shootCount;

	public float maxDeviation;

	public float interval;

	public float minRangeFromSelf = 2.5f;

	public float maxRangeFromSelf;

	public float postDelay;

	public DewAnimationClip castingAnimClip;

	public DewAnimationClip endAnimClip;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		CreateBasicEffect(info.caster, new UnstoppableEffect(), (float)shootCount * interval, "ObliviaxUnstoppable");
		info.caster.Control.StartDaze((float)shootCount * interval + postDelay);
		info.caster.Animation.PlayAbilityAnimation(castingAnimClip);
		Entity target = info.target;
		for (int i = 0; i < shootCount; i++)
		{
			if (target.IsNullInactiveDeadOrKnockedOut())
			{
				target = Dew.GetClosestAliveHero(info.caster.agentPosition, fallbackToDead: true, info.caster);
				if (target.IsNullInactiveDeadOrKnockedOut())
				{
					info.caster.Animation.PlayAbilityAnimation(endAnimClip);
					Destroy();
					yield break;
				}
			}
			Vector3 normalized = (target.GetAIPosition(info.caster) - info.caster.agentPosition).normalized;
			Vector3 vector = AbilityTrigger.PredictPoint_Simple(info.caster, (float)i / (float)shootCount + 0.1f, target, 1f) + Random.onUnitSphere.Flattened().normalized * (Random.value * maxDeviation);
			Vector3 vector2 = (vector - info.caster.position).Flattened();
			if (vector2.sqrMagnitude < minRangeFromSelf * minRangeFromSelf)
			{
				vector2 = vector2.normalized * minRangeFromSelf;
				vector = info.caster.position + vector2;
			}
			else if (vector2.sqrMagnitude >= maxRangeFromSelf * maxRangeFromSelf)
			{
				vector2 = vector2.normalized * Random.Range(maxRangeFromSelf / 1.5f, maxRangeFromSelf);
				vector = info.caster.position + vector2;
			}
			vector = Dew.GetPositionOnGround(vector);
			CreateAbilityInstance<Ai_Mon_Special_BossObliviax_TurretMode_Projectile>(info.caster.position, Quaternion.identity, new CastInfo(info.caster, vector));
			info.caster.Control.Rotate(normalized, immediately: false, interval);
			yield return new SI.WaitForSeconds(interval);
		}
		info.caster.Animation.PlayAbilityAnimation(endAnimClip);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
