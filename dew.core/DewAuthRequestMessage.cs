using Mirror;

public struct DewAuthRequestMessage : NetworkMessage
{
	public string userId;

	public string profileGuid;

	public string profileName;

	public string inviteCode;
}
