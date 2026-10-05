using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Gem_R_Stillness : AbilityInstance
{
	public float procCoefficient = 1f;

	public float stunDuration;

	public ScalingValue damage;

	public ScalingValue scale;

	public Transform[] scaledTransforms;

	public GameObject fxExplosion;

	public GameObject fxHit;

	[Space(10f)]
	public int sweepCount;

	public float sweepInterval;

	public DewCollider range;

	private Vector3[] _originalScales;

	private bool _originalScalesCached;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!_originalScalesCached)
		{
			_originalScales = new Vector3[scaledTransforms.Length];
			for (int i = 0; i < scaledTransforms.Length; i++)
			{
				if (!(scaledTransforms[i] == null))
				{
					_originalScales[i] = scaledTransforms[i].localScale;
				}
			}
			_originalScalesCached = true;
		}
		float num = Mathf.Clamp(GetValue(scale), 0f, 5f);
		for (int j = 0; j < scaledTransforms.Length; j++)
		{
			Transform transform = scaledTransforms[j];
			if (!(transform == null))
			{
				transform.localScale = _originalScales[j] * num;
			}
		}
		FxPlay(fxExplosion);
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		IEnumerable<List<Entity>> enumerable = range.SweepEntitiesFromOrigin(sweepCount, tvDefaultHarmfulEffectTargets);
		foreach (List<Entity> item in enumerable)
		{
			foreach (Entity item2 in item)
			{
				FxPlayNewNetworked(fxHit, item2);
				Damage(damage, procCoefficient).SetElemental(ElementalType.Light).SetOriginPosition(position).Dispatch(item2);
				CreateBasicEffect(item2, new StunEffect(), stunDuration);
			}
			yield return new SI.WaitForSeconds(sweepInterval);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
