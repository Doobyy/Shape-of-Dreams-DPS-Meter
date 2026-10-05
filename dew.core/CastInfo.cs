using System;
using UnityEngine;

public struct CastInfo : IEquatable<CastInfo>
{
	public Entity caster;

	public Entity target;

	public Vector3 point;

	public float angle;

	public float animSelectValue;

	public Vector3 forward => Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

	public Quaternion rotation => Quaternion.Euler(0f, angle, 0f);

	public CastInfo(Entity caster)
	{
		this = default;
		this.caster = caster;
		target = null;
		point = Vector3.zero;
		angle = 0f;
	}

	public CastInfo(Entity caster, Entity target)
	{
		this = default;
		this.caster = caster;
		this.target = target;
	}

	public CastInfo(Entity caster, float angle)
	{
		this = default;
		this.caster = caster;
		this.angle = angle % 360f;
	}

	public CastInfo(Entity caster, Vector3 point)
	{
		this = default;
		this.caster = caster;
		this.point = point;
		angle = GetAngle(point - caster.position);
	}

	public static float GetAngle(Vector3 forward)
	{
		return Vector3.SignedAngle(Vector3.forward, forward.Flattened(), Vector3.up) % 360f;
	}

	public static float GetAngle(Quaternion rotation)
	{
		return rotation.eulerAngles.y;
	}

	public bool Equals(CastInfo other)
	{
		if (caster == other.caster && target == other.target && point.Equals(other.point) && angle.Equals(other.angle))
		{
			return animSelectValue.Equals(other.animSelectValue);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is CastInfo other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((((((((((UnityEngine.Object)(object)caster != null) ? ((object)caster).GetHashCode() : 0) * 397) ^ (((UnityEngine.Object)(object)target != null) ? ((object)target).GetHashCode() : 0)) * 397) ^ point.GetHashCode()) * 397) ^ angle.GetHashCode()) * 397) ^ animSelectValue.GetHashCode();
	}

	public static bool operator ==(CastInfo a, CastInfo b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(CastInfo a, CastInfo b)
	{
		return !a.Equals(b);
	}
}
