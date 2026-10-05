using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class GameCover_Animator : MonoBehaviour
{
	public Vector2 logoOffset = new Vector2(0f, -0.75f);

	public float delay;

	public GameObject fxBeforeDelay;

	public GameObject[] fxPortals;

	public float sustainTime;

	public float decayTime;

	public float targetTimescale;

	public RectTransform logo;

	private void OnEnable()
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			Time.timeScale = 1f;
			Vector3 originalPos = logo.position;
			Vector3 originalScale = logo.localScale;
			logo.localScale = Vector3.one * 0.7f;
			logo.position += new Vector3(logoOffset.x * (float)Screen.width, logoOffset.y * (float)Screen.height, 0f);
			DewEffect.Play(fxBeforeDelay);
			yield return new WaitForSeconds(delay);
			DewEffect.Stop(fxBeforeDelay);
			TweenSettingsExtensions.SetUpdate<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)logo, originalScale, sustainTime + 1.75f), (Ease)18), false);
			TweenSettingsExtensions.SetUpdate<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove((Transform)logo, originalPos, sustainTime + 1.75f, false), (Ease)18), false);
			GameObject[] array = fxPortals;
			for (int i = 0; i < array.Length; i++)
			{
				DewEffect.Play(array[i]);
			}
			yield return new WaitForSecondsRealtime(sustainTime);
			TweenSettingsExtensions.SetUpdate<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => Time.timeScale), (DOSetter<float>)((float v) =>
			{
				Time.timeScale = v;
			}), targetTimescale, decayTime), true);
		}
	}

	private void OnDisable()
	{
		GameObject[] array = fxPortals;
		for (int i = 0; i < array.Length; i++)
		{
			DewEffect.Stop(array[i]);
		}
	}
}
