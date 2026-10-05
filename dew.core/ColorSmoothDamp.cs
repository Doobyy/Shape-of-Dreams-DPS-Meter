using UnityEngine;

public static class ColorSmoothDamp
{
	public static Color SmoothDamp(Color current, Color target, ref Vector4 currentVelocity, float smoothTime)
	{
		return SmoothDamp(current, target, ref currentVelocity, smoothTime, float.PositiveInfinity, Time.deltaTime);
	}

	public static Color SmoothDamp(Color current, Color target, ref Vector4 currentVelocity, float smoothTime, float maxSpeed)
	{
		return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, Time.deltaTime);
	}

	public static Color SmoothDamp(Color current, Color target, ref Vector4 currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
	{
		smoothTime = Mathf.Max(0.0001f, smoothTime);
		float num = 2f / smoothTime;
		Vector4 vector = new Vector4(current.r, current.g, current.b, current.a);
		Vector4 vector2 = new Vector4(target.r, target.g, target.b, target.a);
		float num2 = num * deltaTime;
		float num3 = 1f / (1f + num2 + 0.48f * num2 * num2 + 0.235f * num2 * num2 * num2);
		Vector4 vector3 = vector - vector2;
		Vector4 vector4 = vector2;
		Vector4 vector5 = maxSpeed * smoothTime * Vector4.one;
		vector3.x = Mathf.Clamp(vector3.x, 0f - vector5.x, vector5.x);
		vector3.y = Mathf.Clamp(vector3.y, 0f - vector5.y, vector5.y);
		vector3.z = Mathf.Clamp(vector3.z, 0f - vector5.z, vector5.z);
		vector3.w = Mathf.Clamp(vector3.w, 0f - vector5.w, vector5.w);
		vector2 = vector - vector3;
		Vector4 vector6 = (currentVelocity + num * vector3) * deltaTime;
		currentVelocity = (currentVelocity - num * vector6) * num3;
		Vector4 vector7 = vector2 + (vector3 + vector6) * num3;
		if (vector4.x - vector.x > 0f == vector7.x > vector4.x)
		{
			vector7.x = vector4.x;
			currentVelocity.x = (vector7.x - vector4.x) / deltaTime;
		}
		if (vector4.y - vector.y > 0f == vector7.y > vector4.y)
		{
			vector7.y = vector4.y;
			currentVelocity.y = (vector7.y - vector4.y) / deltaTime;
		}
		if (vector4.z - vector.z > 0f == vector7.z > vector4.z)
		{
			vector7.z = vector4.z;
			currentVelocity.z = (vector7.z - vector4.z) / deltaTime;
		}
		if (vector4.w - vector.w > 0f == vector7.w > vector4.w)
		{
			vector7.w = vector4.w;
			currentVelocity.w = (vector7.w - vector4.w) / deltaTime;
		}
		return new Color(vector7.x, vector7.y, vector7.z, vector7.w);
	}
}
