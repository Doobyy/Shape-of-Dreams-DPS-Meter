using UnityEngine;

public class Title_LastGameCharacters : LogicBehaviour
{
	public CharacterModelDisplay[] models;

	public GameObject displayingObject;

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (models.Length > 1)
		{
			DewGameResult dewGameResult = null;
			if (DewSave.profileMain != null && DewSave.profileMain.lastGameResults != null && DewSave.profileMain.lastGameResults.Count > 0)
			{
				dewGameResult = DewSave.profileMain.lastGameResults[0];
			}
			bool active = false;
			for (int i = 0; i < models.Length; i++)
			{
				if (dewGameResult == null || dewGameResult.players == null || i >= dewGameResult.players.Count)
				{
					models[i].skinType = "";
					continue;
				}
				models[i].skinType = "Skin_" + dewGameResult.players[i].heroType.Substring(5) + "_Default";
				active = true;
			}
			displayingObject.SetActive(active);
		}
		else
		{
			PreferredGameSettings preferredGameSettings = DewSave.profileMain.GetPreferredGameSettings();
			models[0].skinType = "Skin_" + preferredGameSettings.hero.Substring(5) + "_Default";
			displayingObject.SetActive(!string.IsNullOrEmpty(models[0].skinType));
		}
	}
}
