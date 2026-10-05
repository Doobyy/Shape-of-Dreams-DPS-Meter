using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field)]
public class SaveVarAttribute : Attribute
{
	public readonly SaveVarFlags flags;

	public SaveVarAttribute(SaveVarFlags flags = SaveVarFlags.Default)
	{
		this.flags = flags;
	}
}
