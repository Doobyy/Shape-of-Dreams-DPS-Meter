using System;

public class DewGameplaySettings_Platform : ICloneable, IInitializableSettings, IValidatableSettings
{
	public bool disableSoftwareCursor;

	public bool skipIntro;

	public bool skipPhotosensitivityWarning;

	public int travelerStoryFontSizeIndex;

	public int travelerStoryWindowWidthIndex;

	public bool enableCrossPlay = true;

	public bool enableDeveloperMode;

	public void Initialize()
	{
	}

	public void Validate()
	{
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
