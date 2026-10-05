using UnityEngine;

public class ExampleModConfig : ModConfig
{
	public int intTest = 10;

	public float floatTest = 30f;

	public byte byteTest = byte.MaxValue;

	public uint uintTest = 60u;

	public ulong ulongTest = 123uL;

	public ushort ushortTest = 234;

	public string stringTest = "TestString";

	public string nullStringTest;

	public bool toggle1 = true;

	public bool toggle2;

	public Quality4Levels enumTest;

	public Quality4Levels enumTest2 = Quality4Levels.Ultra;

	[LabelText("CUSTOM LABEL WOOHOO!")]
	public bool customLabelTest;

	public Vector2 vector2Test = Vector2.one;

	public Vector3 vector3Test = Vector3.back;

	public Vector4 vector4Test;

	public Vector2Int vector2IntTest;

	public Vector3Int vector3IntTest;

	public Quaternion quaternionTest;
}
