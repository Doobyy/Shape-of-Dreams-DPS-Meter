public class AddedModifierData
{
	public string type;

	public string clientData;

	public bool isForceRevealed;

	public bool revertOnStepEnd;

	public static implicit operator AddedModifierData(string modType)
	{
		return new AddedModifierData
		{
			type = modType
		};
	}
}
