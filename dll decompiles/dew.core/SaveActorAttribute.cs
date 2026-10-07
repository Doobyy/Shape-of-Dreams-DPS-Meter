using System;

[AttributeUsage(AttributeTargets.Class)]
public class SaveActorAttribute : Attribute
{
	public readonly bool shouldBeSaved;

	public SaveActorAttribute(bool shouldBeSaved = true)
	{
		this.shouldBeSaved = shouldBeSaved;
	}
}
