using System;
using UnityEngine;

public struct SyncMovementData : IEquatable<SyncMovementData>
{
	public SyncTransformChannel channels;

	public bool isTeleport;

	public double timestamp;

	public Vector3 position;

	public Vector3 velocity;

	public Quaternion rotation;

	public Vector3 angularVelocity;

	public Vector3 scale;

	public Vector3 scaleVelocity;

	public bool Has(SyncTransformChannel channel)
	{
		return (channels & channel) != 0;
	}

	public bool Equals(SyncMovementData other)
	{
		if (channels == other.channels && isTeleport == other.isTeleport && timestamp.Equals(other.timestamp) && position.Equals(other.position) && velocity.Equals(other.velocity) && rotation.Equals(other.rotation) && angularVelocity.Equals(other.angularVelocity) && scale.Equals(other.scale))
		{
			return scaleVelocity.Equals(other.scaleVelocity);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is SyncMovementData other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		HashCode val = default;
		val.Add<SyncTransformChannel>(channels);
		val.Add<bool>(isTeleport);
		val.Add<double>(timestamp);
		val.Add<Vector3>(position);
		val.Add<Vector3>(velocity);
		val.Add<Quaternion>(rotation);
		val.Add<Vector3>(angularVelocity);
		val.Add<Vector3>(scale);
		val.Add<Vector3>(scaleVelocity);
		return val.ToHashCode();
	}

	public static bool operator ==(SyncMovementData left, SyncMovementData right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SyncMovementData left, SyncMovementData right)
	{
		return !left.Equals(right);
	}
}
