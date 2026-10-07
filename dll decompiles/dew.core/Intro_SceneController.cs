using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class Intro_SceneController : MonoBehaviour
{
	public PlayableDirector director;

	private float _lastSkipButtonShowTime = float.NegativeInfinity;

	private float _normalizedHoldProgress;

	private bool _isInTransition;

	private void Start()
	{
		if (DewSave.platformSettings.gameplay.skipIntro || DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			Dew.CallDelayed(GoToTitle);
			return;
		}
		director.stopped += (PlayableDirector _) =>
		{
			GoToTitle();
		};
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			float waitStartTime = Time.unscaledTime;
			for (int i = 0; i < 5; i++)
			{
				yield return null;
			}
			while (Time.unscaledTime - waitStartTime < 3f && Time.deltaTime > 0.05f)
			{
				yield return null;
			}
			director.Play();
		}
	}

	public void Skip()
	{
		if (!_isInTransition)
		{
			director.Stop();
			GoToTitle();
		}
	}

	public void GoToTitle()
	{
		if (!_isInTransition && !(this == null))
		{
			_isInTransition = true;
			StartCoroutine(Routine());
		}
		static IEnumerator Routine()
		{
			if (ManagerBase<TransitionManager>.instance != null)
			{
				ManagerBase<TransitionManager>.instance.FadeOut(showTips: false);
				yield return new WaitForSecondsRealtime(ManagerBase<TransitionManager>.instance.fadeTime);
			}
			SceneManager.LoadScene("Title");
		}
	}
}
