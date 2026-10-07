using System;
using Mirror;

public static class NetworkConnectionToClientExtension
{
	public static DewPlayer GetPlayer(this NetworkConnectionToClient conn)
	{
		if (!NetworkServer.active)
		{
			throw new Exception("Invalid operation");
		}
		foreach (DewPlayer allHumanPlayer in DewPlayer.allHumanPlayers)
		{
			if (conn == ((NetworkBehaviour)allHumanPlayer).connectionToClient)
			{
				return allHumanPlayer;
			}
		}
		return null;
	}

	public static Hero GetHero(this NetworkConnectionToClient conn)
	{
		if (!NetworkServer.active)
		{
			throw new Exception("Invalid operation");
		}
		foreach (DewPlayer allHumanPlayer in DewPlayer.allHumanPlayers)
		{
			if (conn == ((NetworkBehaviour)allHumanPlayer).connectionToClient)
			{
				return allHumanPlayer.hero;
			}
		}
		return null;
	}
}
