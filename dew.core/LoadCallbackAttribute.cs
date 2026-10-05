using System;

[AttributeUsage(AttributeTargets.Method)]
public class LoadCallbackAttribute : Attribute
{
	public readonly SaveVarFlags flags;

	public LoadCallbackAttribute(SaveVarFlags flags = SaveVarFlags.Default)
	{
		this.flags = flags;
	}
}
