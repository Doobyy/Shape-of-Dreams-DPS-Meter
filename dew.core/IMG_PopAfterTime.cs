using DG.Tweening;
using UnityEngine;

public class IMG_PopAfterTime : MonoBehaviour
{
	public float delay;

	public float sustainTime;

	private Vector3 _ogScale;

	private void Awake()
	{
		_ogScale = transform.localScale;
	}

	private void OnEnable()
	{
		transform.localScale = Vector3.zero;
		DOTween.Kill((object)this, false);
		TweenSettingsExtensions.SetUpdate<Sequence>(TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(DOTween.Sequence((object)this), delay), (Tween)(object)ShortcutExtensions.DOScale(transform, _ogScale, 0.15f)), sustainTime), (Tween)(object)ShortcutExtensions.DOScale(transform, Vector3.zero, 0.15f)), true);
	}

	private void OnDestroy()
	{
		if (!(_ogScale == default(Vector3)) && (bool)this)
		{
			transform.localScale = _ogScale;
		}
	}
}
