using System.Collections.Generic;
using UnityEngine;

public class Ai_Atk_HuskSword_Crit : MeleeAttackInstance
{
	private DewEffect.FxColorSnapshot _startColorSnapshot;

	private DewEffect.FxColorSnapshot _startNoStopColorSnapshot;

	private DewEffect.FxColorSnapshot _endColorSnapshot;

	private Vector3 _baseRangeLocalScale;

	private DewAudioSource[] _audioSources;

	private float[] _audioBaseVolumes;

	private bool _visualBaselineCaptured;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		if (!_visualBaselineCaptured)
		{
			_visualBaselineCaptured = true;
			if (range != null)
			{
				_baseRangeLocalScale = range.transform.localScale;
			}
			if (startEffect != null)
			{
				_startColorSnapshot = DewEffect.CaptureColorsRecursively(startEffect);
			}
			if (startEffectNoStop != null)
			{
				_startNoStopColorSnapshot = DewEffect.CaptureColorsRecursively(startEffectNoStop);
			}
			if (endEffect != null)
			{
				_endColorSnapshot = DewEffect.CaptureColorsRecursively(endEffect);
			}
			List<DewAudioSource> list = new List<DewAudioSource>();
			if (startEffect != null)
			{
				list.AddRange(startEffect.GetComponentsInChildren<DewAudioSource>(includeInactive: true));
			}
			if (startEffectNoStop != null)
			{
				list.AddRange(startEffectNoStop.GetComponentsInChildren<DewAudioSource>(includeInactive: true));
			}
			if (endEffect != null)
			{
				list.AddRange(endEffect.GetComponentsInChildren<DewAudioSource>(includeInactive: true));
			}
			_audioSources = list.ToArray();
			_audioBaseVolumes = new float[_audioSources.Length];
			for (int i = 0; i < _audioSources.Length; i++)
			{
				_audioBaseVolumes[i] = _audioSources[i].volumeMultiplier;
			}
		}
		else
		{
			if (range != null)
			{
				range.transform.localScale = _baseRangeLocalScale;
			}
			DewEffect.RestoreColorsRecursively(_startColorSnapshot);
			DewEffect.RestoreColorsRecursively(_startNoStopColorSnapshot);
			DewEffect.RestoreColorsRecursively(_endColorSnapshot);
			for (int j = 0; j < _audioSources.Length; j++)
			{
				if (_audioSources[j] != null)
				{
					_audioSources[j].volumeMultiplier = _audioBaseVolumes[j];
				}
			}
		}
		base.OnCreate();
	}

	private void MirrorProcessed()
	{
	}
}
