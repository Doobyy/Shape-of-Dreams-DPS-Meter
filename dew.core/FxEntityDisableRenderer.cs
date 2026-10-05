using System.Collections;
using UnityEngine;

public class FxEntityDisableRenderer : MonoBehaviour, IEffectComponent, IAttachableToEntity
{
	public float delay;

	private Entity _target;

	private bool _isRendererDisabled;

	public bool isPlaying => false;

	public void Play()
	{
		if (!_target.IsNullOrInactive())
		{
			StopAllCoroutines();
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			if (delay > 0.0001f)
			{
				yield return new WaitForSeconds(delay);
			}
			if (!_target.IsNullOrInactive() && !_isRendererDisabled)
			{
				_target.Visual.DisableRenderersLocal();
				_isRendererDisabled = true;
			}
		}
	}

	private void ReleaseRendererLock()
	{
		if (_isRendererDisabled)
		{
			_isRendererDisabled = false;
			if ((Object)(object)_target != null)
			{
				_target.Visual.EnableRenderersLocal();
			}
		}
	}

	public void Stop()
	{
		ReleaseRendererLock();
	}

	public void OnAttachToEntity(Entity target)
	{
		ReleaseRendererLock();
		_target = target;
	}
}
