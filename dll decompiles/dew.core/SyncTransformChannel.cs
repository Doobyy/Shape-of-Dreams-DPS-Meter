using System;

[Flags]
public enum SyncTransformChannel : byte
{
	None = 0,
	Position = 1,
	Rotation = 2,
	Scale = 4
}
