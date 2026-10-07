using System;
using Steamworks;

public class SteamTicketForWebApi : IDisposable
{
	public string ticket;

	public HAuthTicket handle;

	public void Dispose()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (handle.m_HAuthTicket != 0)
		{
			if (DewSteam.isInitialized)
			{
				SteamUser.CancelAuthTicket(handle);
			}
			handle.m_HAuthTicket = 0u;
		}
	}
}
