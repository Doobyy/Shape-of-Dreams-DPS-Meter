using System.Collections.Generic;
using System.Threading.Tasks;

public class LobbyInstance
{
	public string id;

	public string name;

	public string version;

	public bool isInviteOnly;

	public string shortCode;

	public bool isModded;

	public string lobbyName;

	public string lobbyDescription;

	public List<string> lobbyTags = new List<string>();

	public string difficulty;

	public List<string> activeLucidDreams = new List<string>();

	public int maxPlayers;

	public List<string> bannedGameItems = new List<string>();

	public AllowMidJoinType allowMidJoins;

	public bool allowDejavu;

	public Dictionary<string, string> customData = new Dictionary<string, string>();

	public bool hasGameStarted;

	public long gameStartTimestamp;

	public bool allowJoin;

	public List<string> savedPlayers = new List<string>();

	public bool isLobbyLeader;

	public int currentPlayers;

	public LobbyConnectionQuality connectionQuality;

	public string gameServerAddress;

	public string hostAddress;

	public string crossPlayGate;

	public string country;

	public override string ToString()
	{
		return GetType().Name + " - " + id;
	}

	public virtual async Task CleanupOnLobbyLeft(string reason)
	{
	}
}
