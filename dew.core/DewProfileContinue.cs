using System;
using System.Collections.Generic;
using UnityEngine;

public class DewProfileContinue
{
	public List<DewPersistence.AchievementsData> achContinueData = new List<DewPersistence.AchievementsData>();

	public string continueData;

	public void Validate()
	{
		if (achContinueData == null)
		{
			achContinueData = new List<DewPersistence.AchievementsData>();
		}
		if (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			continueData = null;
		}
		if (string.IsNullOrEmpty(continueData))
		{
			return;
		}
		try
		{
			DewPersistence.GameData gameData = DewPersistence.FromJson<DewPersistence.GameData>(continueData);
			if (!gameData.Validate())
			{
				continueData = null;
			}
			else
			{
				continueData = DewPersistence.ToJson(gameData);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			continueData = null;
		}
	}
}
