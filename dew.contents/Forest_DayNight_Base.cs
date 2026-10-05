using UnityEngine;

[ExecuteAlways]
public abstract class Forest_DayNight_Base : MonoBehaviour
{
	private Forest_DayNightManager _manager;

	public float animatedValue
	{
		get
		{
			if (_manager == null)
			{
				_manager = GetComponent<Forest_DayNightManager>();
			}
			return _manager.animatedValue;
		}
	}

	public float lastAnimatedValue
	{
		get
		{
			if (_manager == null)
			{
				_manager = GetComponent<Forest_DayNightManager>();
			}
			return _manager.lastAnimatedValue;
		}
	}

	public float targetValue
	{
		get
		{
			if (_manager == null)
			{
				_manager = GetComponent<Forest_DayNightManager>();
			}
			return _manager.targetValue;
		}
	}

	public virtual void Update()
	{
	}

	public virtual void OnTargetValueChanged()
	{
	}
}
