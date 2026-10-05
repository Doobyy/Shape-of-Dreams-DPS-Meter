using System.Collections.Generic;

public class DewPlatformSettings : IInitializableSettings
{
	public const int CurrentSaveVersion = 2;

	public int saveVersion;

	public string lastProfilePath;

	public bool dontShowProfileMigration;

	public List<string> activeMods = new List<string>();

	public bool enableAutoReloadMods;

	public string lastAgreedEulaKeyLocal;

	public DewGraphicsSettings graphics = new DewGraphicsSettings();

	public DewAudioSettings_Platform audio = new DewAudioSettings_Platform();

	public DewGameplaySettings_Platform gameplay = new DewGameplaySettings_Platform();

	public DewControlSettings_Platform controls = new DewControlSettings_Platform();

	public void Initialize()
	{
		saveVersion = 2;
		graphics.Initialize();
		audio.Initialize();
		gameplay.Initialize();
		controls.Initialize();
	}

	public void Validate()
	{
		if (graphics == null)
		{
			graphics = new DewGraphicsSettings();
		}
		if (audio == null)
		{
			audio = new DewAudioSettings_Platform();
		}
		if (gameplay == null)
		{
			gameplay = new DewGameplaySettings_Platform();
		}
		if (controls == null)
		{
			controls = new DewControlSettings_Platform();
		}
		if (activeMods == null)
		{
			activeMods = new List<string>();
		}
		DewSave.ValidateEnumValues(graphics);
		DewSave.ValidateEnumValues(audio);
		DewSave.ValidateEnumValues(gameplay);
		DewSave.ValidateEnumValues(controls);
		graphics.Validate();
		audio.Validate();
		gameplay.Validate();
		controls.Validate();
		if (saveVersion == 0)
		{
			saveVersion = 1;
			gameplay.skipIntro = false;
		}
		if (saveVersion == 1)
		{
			saveVersion = 2;
			gameplay.skipIntro = false;
		}
	}
}
