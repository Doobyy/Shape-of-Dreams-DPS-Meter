using Mirror;

public static class SyncMovementDataSerializer
{
	private const byte TeleportBit = 128;

	public static void WriteSyncMovementData(this NetworkWriter writer, SyncMovementData d)
	{
		byte b = (byte)d.channels;
		if (d.isTeleport)
		{
			b |= 0x80;
		}
		writer.WriteByte(b);
		NetworkWriterExtensions.WriteDouble(writer, d.timestamp);
		if (d.Has(SyncTransformChannel.Position))
		{
			NetworkWriterExtensions.WriteVector3(writer, d.position);
			NetworkWriterExtensions.WriteVector3(writer, d.velocity);
		}
		if (d.Has(SyncTransformChannel.Rotation))
		{
			NetworkWriterExtensions.WriteUInt(writer, Compression.CompressQuaternion(d.rotation));
			NetworkWriterExtensions.WriteVector3(writer, d.angularVelocity);
		}
		if (d.Has(SyncTransformChannel.Scale))
		{
			NetworkWriterExtensions.WriteVector3(writer, d.scale);
			NetworkWriterExtensions.WriteVector3(writer, d.scaleVelocity);
		}
	}

	public static SyncMovementData ReadSyncMovementData(this NetworkReader reader)
	{
		SyncMovementData result = default;
		byte b = reader.ReadByte();
		result.isTeleport = (b & 0x80) != 0;
		result.channels = (SyncTransformChannel)(b & 0x7F);
		result.timestamp = NetworkReaderExtensions.ReadDouble(reader);
		if (result.Has(SyncTransformChannel.Position))
		{
			result.position = NetworkReaderExtensions.ReadVector3(reader);
			result.velocity = NetworkReaderExtensions.ReadVector3(reader);
		}
		if (result.Has(SyncTransformChannel.Rotation))
		{
			result.rotation = Compression.DecompressQuaternion(NetworkReaderExtensions.ReadUInt(reader));
			result.angularVelocity = NetworkReaderExtensions.ReadVector3(reader);
		}
		if (result.Has(SyncTransformChannel.Scale))
		{
			result.scale = NetworkReaderExtensions.ReadVector3(reader);
			result.scaleVelocity = NetworkReaderExtensions.ReadVector3(reader);
		}
		return result;
	}
}
