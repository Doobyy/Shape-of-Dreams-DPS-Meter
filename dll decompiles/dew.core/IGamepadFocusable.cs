using UnityEngine;

public interface IGamepadFocusable : IGamepadFocusListener
{
	FocusableBehavior GetBehavior()
	{
		return FocusableBehavior.Normal;
	}

	bool CanBeFocused()
	{
		if (this is MonoBehaviour { isActiveAndEnabled: not false } monoBehaviour)
		{
			return ManagerBase<GlobalUIManager>.instance.IsUIElementClickable((RectTransform)monoBehaviour.transform);
		}
		return false;
	}

	SelectionDisplayType GetSelectionDisplayType()
	{
		return SelectionDisplayType.Box;
	}

	RectTransform GetTransform()
	{
		if (!((Component)this == null))
		{
			return (RectTransform)((Component)this).transform;
		}
		return null;
	}

	bool CanHoldConfirmToRepeat()
	{
		return false;
	}
}
