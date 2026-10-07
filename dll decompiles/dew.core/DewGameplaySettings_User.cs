using System;
using UnityEngine;

public class DewGameplaySettings_User : ICloneable, IInitializableSettings
{
	public float uiScale = 1f;

	public DamageNumberVisibility damageNumberVisibility = DamageNumberVisibility.All;

	public ReduceOtherPlayerEffectsStrength reduceOtherPlayerEffectsStrength;

	public bool abbreviateBigDamageNumbers = true;

	public bool disableTutorial;

	public float cursorScale = 1f;

	public float cursorColor;

	public bool unlockSkillsOutsideEditMode;

	public float killScreenEffectsStrength = 1f;

	public float screenShakeStrength = 1f;

	public float gamepadVibrationStrength = 1f;

	public bool showHunterWarning = true;

	public bool highlightCharactersOnMouseOver = true;

	public bool enableProfanityFilter = true;

	public bool shareItemsWhenDropped = true;

	public bool enableArachnophobia;

	public ConfirmMemoryEditBehavior confirmMemoryEditByHold = ConfirmMemoryEditBehavior.SellOnly;

	public void Initialize()
	{
		if (SystemInfo.deviceName == "steamdeck")
		{
			uiScale = 1.15f;
			cursorScale = 1.2f;
		}
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
