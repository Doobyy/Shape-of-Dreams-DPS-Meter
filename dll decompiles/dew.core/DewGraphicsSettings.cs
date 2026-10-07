using System;
using UnityEngine;

public class DewGraphicsSettings : ICloneable, IInitializableSettings, IValidatableSettings
{
	public int menuFrameLimit = 60;

	public int gameFrameLimit = -1;

	public bool vSync;

	public int resolutionWidth = Screen.currentResolution.width;

	public int resolutionHeight = Screen.currentResolution.height;

	public FullScreenMode fullScreenMode = FullScreenMode.FullScreenWindow;

	public int displayIndex;

	public Quality4Levels playerAbilityCount = Quality4Levels.Medium;

	public Quality3Levels shaderQuality = Quality3Levels.High;

	public Quality3Levels terrainQuality = Quality3Levels.High;

	public QualityOff3Levels fogQuality = QualityOff3Levels.Medium;

	public Quality3Levels vegetationQuality = Quality3Levels.High;

	public Quality3Levels particleEffectQuality = Quality3Levels.High;

	public QualityOff4Levels shadowQuality = QualityOff4Levels.High;

	public Quality3Levels textureQuality = Quality3Levels.High;

	public bool activityAdaptivePerformance = true;

	public bool enableDynamicResolution = true;

	public void Initialize()
	{
		if (SystemInfo.deviceName == "steamdeck")
		{
			shadowQuality = QualityOff4Levels.Off;
			fogQuality = QualityOff3Levels.Low;
			gameFrameLimit = 60;
		}
	}

	public object Clone()
	{
		return MemberwiseClone();
	}

	public void Validate()
	{
		if (resolutionWidth < 640 || resolutionHeight < 480)
		{
			Resolution currentResolution = Screen.currentResolution;
			resolutionWidth = currentResolution.width;
			resolutionHeight = currentResolution.height;
		}
	}
}
