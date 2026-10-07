using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UI_GamepadFocusable : MonoBehaviour, IGamepadFocusable, IGamepadFocusListener
{
	public SelectionDisplayType display = SelectionDisplayType.Box;

	public bool canHoldConfirmToRepeat;

	public bool focusOnEnable;

	public bool preventAutofocusOnEnable;

	public bool ignoreSelectableInteractivity;

	private bool _isClickable;

	private int _isClickableFrameCount;

	private Selectable _selectable;

	public bool isFocused { get; internal set; }

	public bool VerticalAlignmentLeft { get; set; }

	protected virtual void OnEnable()
	{
		_selectable = (Selectable)(object)GetComponent<Button>();
		if (ManagerBase<GlobalUIManager>.instance == null)
		{
			return;
		}
		ManagerBase<GlobalUIManager>.instance.AddGamepadFocusable(this);
		if (focusOnEnable)
		{
			ManagerBase<GlobalUIManager>.instance.SetFocus(this);
			Dew.CallDelayed(() =>
			{
				ManagerBase<GlobalUIManager>.instance.SetFocus(this);
			});
		}
		else if (preventAutofocusOnEnable)
		{
			ManagerBase<GlobalUIManager>.instance.Unfocus(this);
		}
	}

	public SelectionDisplayType GetSelectionDisplayType()
	{
		return display;
	}

	protected virtual void OnDisable()
	{
		if (ManagerBase<GlobalUIManager>.instance != null)
		{
			ManagerBase<GlobalUIManager>.instance.RemoveGamepadFocusable(this);
		}
	}

	public virtual bool CanBeFocused()
	{
		if (gameObject.activeInHierarchy && (ignoreSelectableInteractivity || (Object)(object)_selectable == null || _selectable.IsInteractable()))
		{
			return ManagerBase<GlobalUIManager>.instance.IsUIElementClickable((RectTransform)transform);
		}
		return false;
	}

	public virtual void OnFocusStateChanged(bool state)
	{
		isFocused = state;
	}

	public bool CanHoldConfirmToRepeat()
	{
		return canHoldConfirmToRepeat;
	}
}
