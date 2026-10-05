using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_E_ClutchesOfMalice : AbilityInstance
{
	public ScalingValue maxTargetCount;

	public Transform[] scaledTransforms;

	private Vector3[] _baseScales;

	public float maxSustainTime;

	public DewCollider range;

	public float additionalRootTime;

	public GameObject fxStart;

	public GameObject fxCancel;

	public GameObject fxComplete;

	public float customStartHeight;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		if (scaledTransforms == null)
		{
			return;
		}
		_baseScales = new Vector3[scaledTransforms.Length];
		for (int i = 0; i < scaledTransforms.Length; i++)
		{
			if (scaledTransforms[i] != null)
			{
				_baseScales[i] = scaledTransforms[i].localScale;
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (scaledTransforms == null || _baseScales == null)
		{
			return;
		}
		for (int i = 0; i < scaledTransforms.Length; i++)
		{
			if (scaledTransforms[i] != null)
			{
				scaledTransforms[i].localScale = _baseScales[i];
			}
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		range.transform.position = info.point;
		St_E_ClutchesOfMalice st_E_ClutchesOfMalice = firstTrigger as St_E_ClutchesOfMalice;
		if ((Object)(object)st_E_ClutchesOfMalice != null)
		{
			for (int i = 0; i < scaledTransforms.Length; i++)
			{
				if (!(scaledTransforms[i] == null))
				{
					scaledTransforms[i].localScale = _baseScales[i] * GetValue(st_E_ClutchesOfMalice.scale);
				}
			}
		}
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		int maxCount = Mathf.RoundToInt(GetValue(maxTargetCount));
		Vector3 startPos = info.point + Vector3.up * customStartHeight;
		float endTime = Time.time + maxSustainTime;
		Entity[] ents = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
		{
			sortComparer = Comparer<Entity>.Create((Entity x, Entity y) => y.Status.currentHealth.CompareTo(x.Status.currentHealth))
		}).ToArray();
		int num = 0;
		for (int num2 = 0; num2 < ents.Length && num2 < maxCount; num2++)
		{
			if (!ents[num2].IsNullInactiveDeadOrKnockedOut())
			{
				num++;
			}
		}
		if (num == 0)
		{
			FxPlayNetworked(fxCancel, startPos, null);
			Destroy();
			yield break;
		}
		float interval = maxSustainTime / (float)num;
		if (interval > 0.05f)
		{
			interval = 0.05f;
		}
		FxPlayNetworked(fxStart, startPos, null);
		int pulledEntities = 0;
		for (int i2 = 0; i2 < ents.Length; i2++)
		{
			if (pulledEntities >= maxCount)
			{
				break;
			}
			Entity entity = ents[i2];
			if (!entity.IsNullInactiveDeadOrKnockedOut())
			{
				CreateAbilityInstance(startPos, null, new CastInfo(info.caster, entity), (Ai_E_ClutchesOfMalice_ReachAndPull b) =>
				{
					b.endSyncTime = endTime;
				});
				CreateStatusEffect(entity, (Se_E_ClutchesOfMalice_Root b) =>
				{
					b.duration = maxSustainTime + additionalRootTime;
				});
				pulledEntities++;
				yield return new SI.WaitForSeconds(interval);
			}
		}
		handle.Return();
		yield return new SI.WaitForSeconds(endTime - Time.time);
		FxStopNetworked(fxStart);
		FxPlayNetworked(fxComplete, startPos, null);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
