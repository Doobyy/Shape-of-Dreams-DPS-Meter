using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Splash_SceneController : MonoBehaviour
{
	public CanvasGroup[] slides;

	public float[] durations;

	public float fadeInTime = 1.25f;

	public float fadeOutTime = 0.65f;

	public float gapInterval = 1f;

	private float _startTime;

	private void OnEnable()
	{
		if (DewSave.platformSettings.gameplay.skipIntro || DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			Dew.CallDelayed(() =>
			{
				SceneManager.LoadScene("Intro");
			});
		}
		else
		{
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			CanvasGroup[] array = slides;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].alpha = 0f;
			}
			yield return new WaitForSecondsRealtime(1.5f);
			for (int j = 0; j < slides.Length; j++)
			{
				while (slides[j].alpha < 1f)
				{
					CanvasGroup obj = slides[j];
					obj.alpha += Time.unscaledDeltaTime / fadeInTime;
					yield return null;
				}
				slides[j].alpha = 1f;
				float remaining = durations[j];
				while (remaining > 0f && !DewInput.GetButtonDown(MouseButton.Left, checkGameArea: false) && !DewInput.GetButtonDown((Key)1) && !DewInput.GetButtonDown((Key)2) && !DewInput.GetButtonDown((GamepadButtonEx?)GamepadButtonEx.A) && !DewInput.GetButtonDown((GamepadButtonEx?)GamepadButtonEx.B) && !DewInput.GetButtonDown((GamepadButtonEx?)GamepadButtonEx.Select) && !DewInput.GetButtonDown((GamepadButtonEx?)GamepadButtonEx.Start))
				{
					remaining -= Time.unscaledDeltaTime;
					yield return null;
				}
				while (slides[j].alpha > 0f)
				{
					CanvasGroup obj2 = slides[j];
					obj2.alpha -= Time.unscaledDeltaTime / fadeOutTime;
					yield return null;
				}
				slides[j].alpha = 0f;
				if (j != slides.Length - 1)
				{
					yield return new WaitForSecondsRealtime(gapInterval);
				}
			}
			SceneManager.LoadScene("Intro");
		}
	}
}
