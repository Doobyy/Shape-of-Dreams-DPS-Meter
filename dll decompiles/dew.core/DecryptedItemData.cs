using System;

public class DecryptedItemData
{
	public string owner;

	public string item;

	public ulong timestamp;

	public ulong expireTimestamp;

	public string ownershipKey;

	public string requiredDLC;

	public bool IsExpired()
	{
		if (expireTimestamp == 0L)
		{
			return false;
		}
		return (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() >= expireTimestamp;
	}

	public bool NeedsRefresh()
	{
		if (string.IsNullOrEmpty(requiredDLC))
		{
			return false;
		}
		if (expireTimestamp == 0L)
		{
			return false;
		}
		ulong num = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		if (num >= expireTimestamp)
		{
			return true;
		}
		return expireTimestamp - num <= 259200000;
	}

	public bool IsDLCNotOwned()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (requiredDLC == "Console")
		{
			return false;
		}
		if (string.IsNullOrEmpty(requiredDLC))
		{
			return false;
		}
		if (!DewSteam.isInitialized)
		{
			return false;
		}
		if (DewSteam.steamId.m_SteamID.ToString() != owner)
		{
			return false;
		}
		return !DewSteam.installedDLCs.Contains(requiredDLC);
	}
}
