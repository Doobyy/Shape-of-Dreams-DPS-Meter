using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class Ai_Star_Mist_F_LG_ChainAttack_Phantom : AbilityInstance
{
	public float dmgRatio = 0.35f;

	public GameObject stabEffect;

	public GameObject lastStabEffect;

	public GameObject stabHitEffect;

	public float stabInterval = 0.12f;

	public float endLinger = 0.3f;

	public float offsetFromTarget = 1.2f;

	[NonSerialized]
	public List<Entity> targets;

	[NonSerialized]
	public float orignDmg;

	[NonSerialized]
	public Vector3 lungeDir;

	[NonSerialized]
	public float retargetRadius = 4f;

	protected override IEnumerator OnCreateSequenced()
	{
		yield return base.OnCreateSequenced();
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		if (targets == null || targets.Count == 0)
		{
			Destroy();
			yield break;
		}
		Vector3 baseDir = lungeDir.Flattened().normalized;
		if (baseDir == Vector3.zero)
		{
			baseDir = ((Component)(object)this).transform.forward;
		}
		Vector3 lastStabPos = ((Component)(object)this).transform.position;
		NavMeshHit val = default;
		for (int i = 0; i < targets.Count; i++)
		{
			Entity entity = targets[i];
			if (entity.IsNullInactiveDeadOrKnockedOut())
			{
				Entity entity2 = FindNewTarget(lastStabPos);
				if ((UnityEngine.Object)(object)entity2 == null)
				{
					continue;
				}
				for (int j = i + 1; j < targets.Count; j++)
				{
					if ((UnityEngine.Object)(object)targets[j] == (UnityEngine.Object)(object)entity)
					{
						targets[j] = entity2;
					}
				}
				entity = entity2;
			}
			lastStabPos = entity.position;
			float num = 90f * (float)(i + 1) + UnityEngine.Random.Range(-10f, 10f);
			Vector3 forward = (Quaternion.Euler(0f, num, 0f) * baseDir).normalized;
			Vector3 vector = entity.position;
			for (float num2 = 0f; num2 < 360f; num2 += 20f)
			{
				Vector3 normalized = (Quaternion.Euler(0f, num + num2, 0f) * baseDir).normalized;
				if (NavMesh.SamplePosition(entity.position - normalized * offsetFromTarget, ref val, 0.6f, -1))
				{
					forward = normalized;
					vector = val.position;
					break;
				}
			}
			Quaternion value = Quaternion.LookRotation(forward).Flattened();
			FxPlayNewNetworked((i == targets.Count - 1) ? lastStabEffect : stabEffect, vector, value);
			FxPlayNewNetworked(stabHitEffect, entity);
			PureDamage(dmgRatio * orignDmg).DoAttackEffect(AttackEffectType.Others).Dispatch(entity);
			yield return new SI.WaitForSeconds(stabInterval);
		}
		yield return new SI.WaitForSeconds(endLinger);
		Destroy();
	}

	private Entity FindNewTarget(Vector3 center)
	{
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, center, retargetRadius, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		Entity result = null;
		foreach (Entity item in list)
		{
			if (!item.IsNullInactiveDeadOrKnockedOut())
			{
				result = item;
				break;
			}
		}
		handle.Return();
		return result;
	}

	private void MirrorProcessed()
	{
	}
}
