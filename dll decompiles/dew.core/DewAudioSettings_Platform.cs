using System;

public class DewAudioSettings_Platform : ICloneable, IInitializableSettings, IValidatableSettings
{
	public bool useAdvancedPrioritization = true;

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
