using Mirror;

public struct DewAuthResponseMessage : NetworkMessage
{
	public bool isError;

	public DewExceptionType errorType;
}
