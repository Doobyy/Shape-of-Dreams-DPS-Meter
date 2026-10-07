using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;

public class PreferredGameSettings
{
	public string difficulty = "diffNormal";

	public string hero = "Hero_Lacerta";

	public string lobbyName = "";

	public string lobbyDescription = "";

	public string dejavuItem = "";

	public List<string> lucidDreams = new List<string>();

	public List<string> bannedGameItems = new List<string>();

	public List<string> tags = new List<string>();

	public bool enableVotes = true;

	public bool allowDejavu = true;

	public AllowMidJoinType allowMidJoins;

	public Dictionary<string, string> customData = new Dictionary<string, string>();

	public Dictionary<string, int> heroSelectedLoadoutIndex = new Dictionary<string, int>();

	public void Validate()
	{
		if (lucidDreams == null)
		{
			lucidDreams = new List<string>();
		}
		if (bannedGameItems == null)
		{
			bannedGameItems = new List<string>();
		}
		if (tags == null)
		{
			tags = new List<string>();
		}
		if (customData == null)
		{
			customData = new Dictionary<string, string>();
		}
		if (heroSelectedLoadoutIndex == null)
		{
			heroSelectedLoadoutIndex = new Dictionary<string, int>();
		}
		foreach (Type allHero in Dew.allHeroes)
		{
			if (!heroSelectedLoadoutIndex.ContainsKey(allHero.Name))
			{
				heroSelectedLoadoutIndex.Add(allHero.Name, 0);
			}
		}
		string[] array = heroSelectedLoadoutIndex.Keys.ToArray();
		foreach (string key in array)
		{
			if (heroSelectedLoadoutIndex[key] < 0 || heroSelectedLoadoutIndex[key] >= 5)
			{
				heroSelectedLoadoutIndex[key] = 0;
			}
		}
	}

	public void ApplyToGame()
	{
		GameSettingsManager instance = NetworkedManagerBase<GameSettingsManager>.instance;
		if (NetworkServer.dontListen)
		{
			instance.allowDejavu = true;
		}
		else
		{
			instance.allowDejavu = allowDejavu;
		}
		instance.allowMidJoins = allowMidJoins;
		instance.enableVotes = enableVotes;
		instance.difficulty = difficulty;
		instance.ClearLucidDreams();
		foreach (string lucidDream in lucidDreams)
		{
			instance.AddLucidDream(lucidDream);
		}
		instance.bannedGameItems.Clear();
		instance.bannedGameItems.AddRange((IEnumerable<string>)bannedGameItems);
		instance.lobbyTags.Clear();
		instance.lobbyTags.AddRange((IEnumerable<string>)tags);
		instance.lobbyName = lobbyName;
		instance.lobbyDescription = lobbyDescription;
		foreach (KeyValuePair<string, string> customDatum in customData)
		{
			((SyncIDictionary<string, string>)(object)instance.customData)[customDatum.Key] = customDatum.Value;
		}
	}
}
