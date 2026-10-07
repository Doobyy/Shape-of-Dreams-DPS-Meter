using System;

[Serializable]
public struct ModifierData(string type)
{
	public int id = 0;

	public string type = type;

	public string clientData = "";

	public bool isForceRevealed = false;
}
