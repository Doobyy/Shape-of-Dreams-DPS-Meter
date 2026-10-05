using System;
using System.Collections;
using UnityEngine;

[ExecuteAlways]
public class Forest_DayNightManager : MonoBehaviour
{
	public float smoothTime;

	public bool isDayOnStart;

	public bool setDayWhenSafe;

	private float _cv;

	private Forest_DayNight_Base[] _components;

	public float targetValue { get; private set; }

	public float animatedValue { get; private set; }

	public float lastAnimatedValue { get; private set; }

	private void Awake()
	{
		targetValue = ((!isDayOnStart) ? 1 : 0);
		animatedValue = ((!isDayOnStart) ? 1 : 0);
	}

	private void Start()
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			while ((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.softInstance == null || (UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.softInstance == null)
			{
				yield return null;
			}
			if (Application.IsPlaying(this))
			{
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(OnRoomLoaded);
			}
		}
	}

	private void OnDestroy()
	{
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(OnRoomLoaded);
		}
	}

	private void OnRoomLoaded(EventInfoLoadRoom _)
	{
		if (isDayOnStart)
		{
			SetDay(immediately: true);
		}
		else
		{
			SetNight(immediately: true);
		}
	}

	private void OnIsSafeZoneChanged(bool arg1, bool isSafe)
	{
		if (isSafe && setDayWhenSafe)
		{
			SetDay(immediately: false);
		}
		else
		{
			SetNight(immediately: false);
		}
	}

	public void ToggleDayNight()
	{
		if (Mathf.RoundToInt(targetValue) % 2 == 1)
		{
			SetDay(immediately: false);
		}
		else
		{
			SetNight(immediately: false);
		}
	}

	public void SetDay(bool immediately)
	{
		if (Mathf.RoundToInt(targetValue) % 2 == 1)
		{
			SetTargetValue(targetValue + 1f);
		}
		if (!Application.IsPlaying(this) | immediately)
		{
			SetAnimatedValue(targetValue);
		}
	}

	public void SetNight(bool immediately)
	{
		if (Mathf.RoundToInt(targetValue) % 2 == 0)
		{
			SetTargetValue(targetValue + 1f);
		}
		if (!Application.IsPlaying(this) | immediately)
		{
			SetAnimatedValue(targetValue);
		}
	}

	private void SetTargetValue(float newValue)
	{
		if (!(Math.Abs(targetValue - newValue) < 0.001f))
		{
			targetValue = newValue;
			if (_components == null)
			{
				_components = GetComponentsInChildren<Forest_DayNight_Base>(includeInactive: true);
			}
			Forest_DayNight_Base[] components = _components;
			for (int i = 0; i < components.Length; i++)
			{
				components[i].OnTargetValueChanged();
			}
		}
	}

	private void Update()
	{
		SetAnimatedValue(Mathf.SmoothDamp(animatedValue, targetValue, ref _cv, smoothTime));
	}

	private void SetAnimatedValue(float newValue)
	{
		lastAnimatedValue = animatedValue;
		animatedValue = newValue;
	}
}
