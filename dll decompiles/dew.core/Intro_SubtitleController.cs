using System.Collections;
using TMPro;
using UnityEngine;

public class Intro_SubtitleController : MonoBehaviour
{
	public float showDurationAfterDone;

	public TextMeshProUGUI text;

	public GameObject subtitleObject;

	private int _nextSubtitle;

	private void Start()
	{
		subtitleObject.SetActive(value: false);
	}

	public void AdvanceSubtitle()
	{
		AdvanceSubtitle(showDurationAfterDone);
	}

	public void AdvanceSubtitle(float duration)
	{
		StopAllCoroutines();
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			subtitleObject.SetActive(value: true);
			((TMP_Text)text).text = DewLocalization.GetUIValue($"NewIntroVideo_Subtitle_{_nextSubtitle:00}");
			_nextSubtitle++;
			yield return new WaitForSecondsRealtime(duration);
			subtitleObject.SetActive(value: false);
		}
	}
}
