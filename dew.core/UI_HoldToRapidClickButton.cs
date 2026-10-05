using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_HoldToRapidClickButton : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	public UnityEvent onClick;

	private bool _isHeld;

	private int _clickCount;

	private float _lastClickUnscaledTime;

	private void Update()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected Obj, but got Unknown
		if (!_isHeld)
		{
			return;
		}
		Selectable component = GetComponent<Selectable>();
		if ((UnityEngine.Object)(object)component != null && !component.IsInteractable())
		{
			OnPointerUp(new PointerEventData(EventSystem.current)
			{
				button = (InputButton)0
			});
			return;
		}
		float num = Time.unscaledTime - _lastClickUnscaledTime;
		if (_clickCount < 4)
		{
			if (num > 0.3f)
			{
				Click();
			}
		}
		else if (_clickCount < 8)
		{
			if (num > 0.2f)
			{
				Click();
			}
		}
		else if (_clickCount < 20)
		{
			if (num > 0.1f)
			{
				Click();
			}
		}
		else if (_clickCount < 30)
		{
			if (num > 0.05f)
			{
				Click();
			}
		}
		else if (_clickCount < 50)
		{
			if (num > 0.025f)
			{
				Click();
			}
		}
		else if (num > 0.02f)
		{
			Click();
		}
	}

	private void OnDisable()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected Obj, but got Unknown
		OnPointerUp(new PointerEventData(EventSystem.current)
		{
			button = (InputButton)0
		});
	}

	private void Click()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected Obj, but got Unknown
		_clickCount++;
		_lastClickUnscaledTime = Time.unscaledTime;
		PointerEventData val = new PointerEventData(EventSystem.current)
		{
			button = (InputButton)0
		};
		IPointerDownHandler[] components = GetComponents<IPointerDownHandler>();
		foreach (IPointerDownHandler val2 in components)
		{
			if ((object)val2 != this)
			{
				val2.OnPointerDown(val);
			}
		}
		IPointerUpHandler[] components2 = GetComponents<IPointerUpHandler>();
		foreach (IPointerUpHandler val3 in components2)
		{
			if ((object)val3 != this)
			{
				val3.OnPointerUp(val);
			}
		}
		IPointerClickHandler[] components3 = GetComponents<IPointerClickHandler>();
		foreach (IPointerClickHandler val4 in components3)
		{
			if ((object)val4 != this)
			{
				val4.OnPointerClick(val);
			}
		}
		try
		{
			onClick?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if ((int)eventData.button == 0)
		{
			Selectable component = GetComponent<Selectable>();
			if (!((UnityEngine.Object)(object)component != null) || component.IsInteractable())
			{
				_clickCount = 0;
				_isHeld = true;
				_lastClickUnscaledTime = float.NegativeInfinity;
				Update();
			}
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if ((int)eventData.button == 0 && _isHeld)
		{
			_clickCount = 0;
			_isHeld = false;
			_lastClickUnscaledTime = float.NegativeInfinity;
		}
	}
}
