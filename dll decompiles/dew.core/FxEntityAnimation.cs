using System.Collections;
using Mirror;
using UnityEngine;

public class FxEntityAnimation : MonoBehaviour, IEffectComponent, IAttachableToEntity
{
	public DewAnimationClip animClip;

	public float delay;

	public float speed = 1f;

	public bool stopAnimationWhenStopped;

	private Entity _target;

	public bool isPlaying => false;

	public void Play()
	{
		if (NetworkServer.active && !(animClip == null) && !_target.IsNullOrInactive())
		{
			StopAllCoroutines();
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(delay);
			if (!_target.IsNullOrInactive())
			{
				_target.Animation.PlayAbilityAnimation(animClip, speed);
			}
		}
	}

	public void Stop()
	{
		if (NetworkServer.active && !(animClip == null) && !((Object)(object)_target == null) && stopAnimationWhenStopped)
		{
			StopAllCoroutines();
			_target.Animation.StopAbilityAnimation(animClip);
		}
	}

	public void OnAttachToEntity(Entity target)
	{
		_target = target;
	}
}
