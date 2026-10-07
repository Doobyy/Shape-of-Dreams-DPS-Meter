using System.Globalization;
using Steamworks;

public class LobbyInstanceSteam : LobbyInstance
{
	public void SetCSteamId(CSteamID lobbyId)
	{
		id = lobbyId.m_SteamID.ToString(CultureInfo.InvariantCulture);
	}

	public CSteamID GetCSteamId()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (!ulong.TryParse(id, out var result))
		{
			return default;
		}
		return new CSteamID(result);
	}
}
