using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class Intro_VideoIntroController : MonoBehaviour
{
	public Intro_SubtitleController subtitle;

	public VideoPlayer video;

	[Multiline(20)]
	public string subtitleTimings;

	private int _nextSubtitle;

	private bool _isInTransition;

	private void Awake()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected Obj, but got Unknown
		if (DewSave.platformSettings.gameplay.skipIntro || DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			Dew.CallDelayed(GoToTitle);
			return;
		}
		video.loopPointReached += (VideoPlayer _) =>
		{
			Skip();
		};
	}

	private void Update()
	{
		float[] array = (from s in subtitleTimings.Trim().Replace("~", "\n").Split("\n", StringSplitOptions.None)
			select float.Parse(s.Trim(), CultureInfo.InvariantCulture)).ToArray();
		if (_nextSubtitle >= 0 && _nextSubtitle < array.Length / 2 && video.time > (double)(array[_nextSubtitle * 2] + 1f))
		{
			subtitle.AdvanceSubtitle(array[_nextSubtitle * 2 + 1] - array[_nextSubtitle * 2]);
			_nextSubtitle++;
		}
	}

	public void Skip()
	{
		if (!_isInTransition)
		{
			video.Pause();
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
