using Mirror;

public static class SampleCastInfoContextSerialization
{
	public static void WriteSampleCastInfoContext(this NetworkWriter writer, SampleCastInfoContext? value)
	{
		if (!value.HasValue)
		{
			NetworkWriterExtensions.WriteBool(writer, false);
			return;
		}
		NetworkWriterExtensions.WriteBool(writer, true);
		writer.Write<SampleCastInfoContext>(value.Value);
	}

	public static SampleCastInfoContext? ReadSampleCastInfoContext(this NetworkReader reader)
	{
		if (!NetworkReaderExtensions.ReadBool(reader))
		{
			return null;
		}
		return reader.Read<SampleCastInfoContext>();
	}
}
