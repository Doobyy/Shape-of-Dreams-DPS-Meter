using UnityEngine;

public interface IGamepadFocusListener
{
	bool IsValid()
	{
		if (this is Component component)
		{
			return component != null;
		}
		return false;
	}

	void OnFocusStateChanged(bool state)
	{
	}
}
