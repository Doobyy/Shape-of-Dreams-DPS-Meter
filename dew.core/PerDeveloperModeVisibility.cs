using UnityEngine;

[RequireComponent(typeof(VisibilityController))]
public class PerDeveloperModeVisibility : MonoBehaviour, ISettingsChangedCallback, IVisibilityComponent
{
	public bool showIfDeveloper = true;

	public bool showIfNotDeveloper;

	private VisibilityController _controller;

	bool IVisibilityComponent.shouldBeShown
	{
		get
		{
			bool enableDeveloperMode = DewSave.platformSettings.gameplay.enableDeveloperMode;
			if (!enableDeveloperMode || !showIfDeveloper)
			{
				if (!enableDeveloperMode)
				{
					return showIfNotDeveloper;
				}
				return false;
			}
			return true;
		}
	}

	private void Start()
	{
		_controller = GetComponent<VisibilityController>();
		_controller.AddVisbilityComponent(this);
		_controller.UpdateVisibility();
	}

	private void OnDestroy()
	{
		if ((bool)_controller)
		{
			_controller.RemoveVisbilityComponent(this);
		}
	}

	public void OnSettingsChanged()
	{
		if ((bool)_controller)
		{
			_controller.UpdateVisibility();
		}
	}
}
