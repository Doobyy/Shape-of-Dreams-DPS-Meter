using System;
using UnityEngine;

public class PerLaunchOptionVisibility : MonoBehaviour
{
	[Flags]
	public enum LaunchOptions
	{
		None = 0,
		forceKeyboardAndMouse = 1,
		forceGamepad = 2
	}

	public bool alwaysShowInEditor = true;

	public LaunchOptions showIf;

	public LaunchOptions hideIf;

	private void Awake()
	{
		if (!ShouldShowGameObject())
		{
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	private bool ShouldShowGameObject()
	{
		if (hideIf != LaunchOptions.None && HasAnyFlag(hideIf))
		{
			return false;
		}
		if (showIf != LaunchOptions.None)
		{
			return HasAnyFlag(showIf);
		}
		return true;
	}

	private bool HasAnyFlag(LaunchOptions flags)
	{
		return (GetCurrentLaunchOptions() & flags) != 0;
	}

	private LaunchOptions GetCurrentLaunchOptions()
	{
		LaunchOptions launchOptions = LaunchOptions.None;
		if (DewLaunchOptions.forceKeyboardAndMouse)
		{
			launchOptions |= LaunchOptions.forceKeyboardAndMouse;
		}
		if (DewLaunchOptions.forceGamepad)
		{
			launchOptions |= LaunchOptions.forceGamepad;
		}
		return launchOptions;
	}
}
