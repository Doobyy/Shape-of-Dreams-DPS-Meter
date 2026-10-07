using Mirror;

public static class CastMethodDataSerialization
{
	public static void WriteCastMethodData(this NetworkWriter writer, CastMethodData value)
	{
		writer.Write<byte>((byte)value.type);
		writer.Write<float>(value._angle);
		writer.Write<float>(value._length);
		writer.Write<float>(value._width);
		writer.Write<float>(value._range);
		writer.Write<float>(value._radius);
		writer.Write<bool>(value._isClamping);
	}

	public static CastMethodData ReadWriteCastMethodData(this NetworkReader reader)
	{
		return new CastMethodData
		{
			type = (CastMethodType)reader.ReadByte(),
			_angle = NetworkReaderExtensions.ReadFloat(reader),
			_length = NetworkReaderExtensions.ReadFloat(reader),
			_width = NetworkReaderExtensions.ReadFloat(reader),
			_range = NetworkReaderExtensions.ReadFloat(reader),
			_radius = NetworkReaderExtensions.ReadFloat(reader),
			_isClamping = NetworkReaderExtensions.ReadBool(reader)
		};
	}
}
