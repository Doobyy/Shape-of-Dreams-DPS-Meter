using System.Collections.Generic;

public class DewNetworkStartSettings
{
	public DewNetworkMode networkMode;

	public bool lanMode;

	public DewPersistence.GameData continueData;

	public DewLobbyType lobbyType;

	public string lobbyName = "";

	public int maxPlayers = 4;

	public List<string> addedGameMods = new List<string>();

	public string customGameSettingsSaveKey;

	public string address;
}
