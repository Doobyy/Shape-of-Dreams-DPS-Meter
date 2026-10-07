using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Gem_R_Glaciate : AbilityInstance
{
	public float procCoefficient = 1f;

	public ScalingValue damage;

	public ScalingValue scale;

	public Transform[] scaledTransforms;

	public GameObject glaciateEffect;

	public GameObject hitEffect;

	public int sweepCount;

	public float sweepInterval;

	public DewCollider range;

	private Vector3[] _scaledTransformsOriginalScale;

	private bool _scaledTransformsOriginalScaleCached;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!_scaledTransformsOriginalScaleCached)
		{
			_scaledTransformsOriginalScale = new Vector3[scaledTransforms.Length];
			for (int i = 0; i < scaledTransforms.Length; i++)
			{
				if (!(scaledTransforms[i] == null))
				{
					_scaledTransformsOriginalScale[i] = scaledTransforms[i].localScale;
				}
			}
			_scaledTransformsOriginalScaleCached = true;
		}
		float num = Mathf.Clamp(GetValue(scale), 0f, 5f);
		for (int j = 0; j < scaledTransforms.Length; j++)
		{
			if (!(scaledTransforms[j] == null))
			{
				scaledTransforms[j].localScale = _scaledTransformsOriginalScale[j] * num;
			}
		}
		FxPlay(glaciateEffect);
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		IEnumerable<List<Entity>> enumerable = range.SweepEntitiesFromOrigin(sweepCount, tvDefaultHarmfulEffectTargets);
		foreach (List<Entity> item in enumerable)
		{
			foreach (Entity item2 in item)
			{
				FxPlayNewNetworked(hitEffect, item2);
				Damage(damage, procCoefficient).SetElemental(ElementalType.Cold).SetOriginPosition(position).Dispatch(item2);
			}
			yield return new SI.WaitForSeconds(sweepInterval);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
