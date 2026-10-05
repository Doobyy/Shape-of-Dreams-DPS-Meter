using System.Collections;
using UnityEngine;

public class FxAnimatorSetFloat : MonoBehaviour, IEffectComponent, IAttachableToEntity
{
	public float delay;

	public Animator animator;

	public string parameter;

	public AnimationCurve value;

	public bool isPlaying => false;

	public void Play()
	{
		StopAllCoroutines();
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (delay > 0f)
			{
				yield return new WaitForSeconds(delay);
			}
			if ((Object)(object)animator == null)
			{
				animator = GetComponentInChildren<Animator>();
			}
			float startTime = Time.time;
			while (!((Object)(object)animator == null))
			{
				animator.SetFloat(parameter, value.Evaluate(Time.time - startTime));
				yield return null;
			}
		}
	}

	public void Stop()
	{
		StopAllCoroutines();
	}

	public void OnAttachToEntity(Entity target)
	{
		animator = (((Object)(object)target == null) ? null : target.Animation.animator);
	}
}
