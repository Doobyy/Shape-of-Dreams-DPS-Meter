using System.Collections;
using UnityEngine;

public class FxAnimatorSetTrigger : MonoBehaviour, IEffectComponent, IAttachableToEntity
{
	public float delay;

	public Animator animator;

	private Animator _entityAnimator;

	public string trigger;

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
			if ((Object)(object)animator != null)
			{
				animator.SetTrigger(trigger);
			}
			else if ((Object)(object)_entityAnimator != null)
			{
				_entityAnimator.SetTrigger(trigger);
			}
		}
	}

	public void Stop()
	{
	}

	public void OnAttachToEntity(Entity target)
	{
		_entityAnimator = (((Object)(object)target == null) ? null : target.Animation.animator);
	}
}
