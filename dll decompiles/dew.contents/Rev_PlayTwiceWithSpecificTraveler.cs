using System.Collections.Generic;
using UnityEngine;

public class Rev_PlayTwiceWithSpecificTraveler : DewReverieItem
{
	[AchPersistentVar]
	private string _travelerType;

	[AchPersistentVar]
	private int _playCount;

	public override int grantedStardust => 30;

	public string travelerName => DewLocalization.GetUIValue(_travelerType + "_Name");

	public override int GetCurrentProgress()
	{
		return _playCount;
	}

	public override int GetMaxProgress()
	{
		return 2;
	}

	public override void OnSetupReverie()
	{
		base.OnSetupReverie();
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, DewProfile.UnlockData> hero in DewSave.profileMain.heroes)
		{
			if (Dew.IsHeroIncludedInGame(hero.Key) && hero.Value.status != UnlockStatus.Locked)
			{
				list.Add(hero.Key);
			}
		}
		if (list.Count > 0)
		{
			_travelerType = list[Random.Range(0, list.Count)];
		}
		else
		{
			_travelerType = "Hero_Lacerta";
		}
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (!(((object)hero).GetType().Name != _travelerType))
		{
			_playCount++;
			if (_playCount >= 2)
			{
				Complete();
			}
		}
	}
}
