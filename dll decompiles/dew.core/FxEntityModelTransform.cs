using System.Collections;
using UnityEngine;

public class FxEntityModelTransform : MonoBehaviour, IEffectComponent, IAttachableToEntity
{
	public Transform from;

	public Transform to;

	public float duration;

	public AnimationCurve normalizedLerpCurve;

	private Transform _model;

	public bool isPlaying { get; }

	public void Play()
	{
		if (!(_model == null))
		{
			StopAllCoroutines();
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			if (from != null)
			{
				_model.position = from.position;
				_model.rotation = from.rotation;
			}
			Vector3 fromPos = _model.position;
			Quaternion fromRot = _model.rotation;
			for (float t = 0f; t < duration; t += Time.deltaTime)
			{
				yield return null;
				if (to == null)
				{
					yield break;
				}
				float time = t / duration;
				time = normalizedLerpCurve.Evaluate(time);
				_model.position = Vector3.Lerp(fromPos, to.position, time);
				_model.rotation = Quaternion.Lerp(fromRot, to.rotation, time);
			}
			if (!(to == null))
			{
				_model.position = to.position;
				_model.rotation = to.rotation;
			}
		}
	}

	public void Stop()
	{
		StopAllCoroutines();
		if (_model != null && to != null)
		{
			_model.position = to.position;
			_model.rotation = to.rotation;
		}
	}

	public void OnAttachToEntity(Entity target)
	{
		if (!((Object)(object)target == null))
		{
			_model = target.Visual.model.transform;
		}
	}
}
