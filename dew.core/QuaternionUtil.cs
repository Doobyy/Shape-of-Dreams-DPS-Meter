using UnityEngine;

public static class QuaternionUtil
{
	public static Quaternion AngVelToDeriv(Quaternion Current, Vector3 AngVel)
	{
		Quaternion quaternion = new Quaternion(AngVel.x, AngVel.y, AngVel.z, 0f) * Current;
		return new Quaternion(0.5f * quaternion.x, 0.5f * quaternion.y, 0.5f * quaternion.z, 0.5f * quaternion.w);
	}

	public static Vector3 DerivToAngVel(Quaternion Current, Quaternion Deriv)
	{
		Quaternion quaternion = Deriv * Quaternion.Inverse(Current);
		return new Vector3(2f * quaternion.x, 2f * quaternion.y, 2f * quaternion.z);
	}

	public static Quaternion IntegrateRotation(Quaternion Rotation, Vector3 AngularVelocity, float DeltaTime)
	{
		if (DeltaTime < Mathf.Epsilon)
		{
			return Rotation;
		}
		Quaternion quaternion = AngVelToDeriv(Rotation, AngularVelocity);
		Vector4 normalized = new Vector4(Rotation.x + quaternion.x * DeltaTime, Rotation.y + quaternion.y * DeltaTime, Rotation.z + quaternion.z * DeltaTime, Rotation.w + quaternion.w * DeltaTime).normalized;
		return new Quaternion(normalized.x, normalized.y, normalized.z, normalized.w);
	}

	public static Quaternion SmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float time)
	{
		if (Time.deltaTime < Mathf.Epsilon)
		{
			return rot;
		}
		float num = ((Quaternion.Dot(rot, target) > 0f) ? 1f : (-1f));
		target.x *= num;
		target.y *= num;
		target.z *= num;
		target.w *= num;
		Vector4 normalized = new Vector4(Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time), Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time), Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time), Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time)).normalized;
		Vector4 vector = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), normalized);
		deriv.x -= vector.x;
		deriv.y -= vector.y;
		deriv.z -= vector.z;
		deriv.w -= vector.w;
		return new Quaternion(normalized.x, normalized.y, normalized.z, normalized.w);
	}
}
