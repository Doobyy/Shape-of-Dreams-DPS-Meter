using System;
using System.Threading;
using UnityEngine;

public class DewRandom
{
	public static DewRandom instance = new DewRandom(GetRandomSeed());

	public uint s0;

	public uint s1;

	public uint s2;

	public uint s3;

	public uint initialSeed;

	private static int s_counter = 0;

	public override string ToString()
	{
		return $"DewRandom({initialSeed}, {s0}, {s1}, {s2}, {s3})";
	}

	public static uint GetRandomSeed()
	{
		return (uint)(++s_counter ^ Environment.TickCount ^ Thread.CurrentThread.ManagedThreadId ^ (int)DateTime.UtcNow.Ticks);
	}

	public DewRandom(uint seed)
	{
		SetSeed(seed);
	}

	public void SetSeed(uint seed)
	{
		initialSeed = seed;
		s0 = SplitMix32(ref seed);
		s1 = SplitMix32(ref seed);
		s2 = SplitMix32(ref seed);
		s3 = SplitMix32(ref seed);
	}

	private static uint SplitMix32(ref uint x)
	{
		uint num = (x += 2654435769u);
		int num2 = (int)(num ^ (num >> 16)) * -2048144789;
		int num3 = (num2 ^ (num2 >>> 13)) * -1028477387;
		return (uint)(num3 ^ (num3 >>> 16));
	}

	private static uint Rotl(uint x, int k)
	{
		return (x << k) | (x >> 32 - k);
	}

	public uint NextUInt32()
	{
		uint result = Rotl(s1 * 5, 7) * 9;
		uint num = s1 << 9;
		s2 ^= s0;
		s3 ^= s1;
		s1 ^= s2;
		s0 ^= s3;
		s2 ^= num;
		s3 = Rotl(s3, 11);
		return result;
	}

	public float NextFloat()
	{
		return (float)(NextUInt32() >> 8) * 5.9604645E-08f;
	}

	public double NextDouble()
	{
		ulong num = NextUInt32() >> 5;
		ulong num2 = NextUInt32() >> 6;
		return ((double)num * 67108864.0 + (double)num2) / 9007199254740992.0;
	}

	public float NextFloat(float min, float max)
	{
		return min + (max - min) * NextFloat();
	}

	public Vector2 InsideUnitCircle()
	{
		float f = NextFloat() * (float)Math.PI * 2f;
		float num = Mathf.Sqrt(NextFloat());
		return new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * num;
	}

	public Vector3 InsideUnitSphere()
	{
		Vector3 result;
		do
		{
			float x = NextFloat(-1f, 1f);
			float y = NextFloat(-1f, 1f);
			float z = NextFloat(-1f, 1f);
			result = new Vector3(x, y, z);
		}
		while (!(result.sqrMagnitude <= 1f));
		return result;
	}

	public Vector3 OnUnitSphere()
	{
		float num = NextFloat(-1f, 1f);
		float f = NextFloat() * 2f * (float)Math.PI;
		float num2 = Mathf.Sqrt(1f - num * num);
		float x = num2 * Mathf.Cos(f);
		float y = num2 * Mathf.Sin(f);
		return new Vector3(x, y, num);
	}

	public Quaternion Rotation()
	{
		return Quaternion.Euler(NextFloat(0f, 360f), NextFloat(0f, 360f), NextFloat(0f, 360f));
	}

	public Quaternion RotationUniform()
	{
		float num = NextFloat();
		float num2 = NextFloat();
		float num3 = NextFloat();
		float num4 = Mathf.Sqrt(1f - num);
		float num5 = Mathf.Sqrt(num);
		float f = (float)Math.PI * 2f * num2;
		float f2 = (float)Math.PI * 2f * num3;
		float x = Mathf.Sin(f) * num4;
		float y = Mathf.Cos(f) * num4;
		float z = Mathf.Sin(f2) * num5;
		float w = Mathf.Cos(f2) * num5;
		return new Quaternion(x, y, z, w);
	}

	public Color ColorHSV(float hueMin = 0f, float hueMax = 1f, float satMin = 0f, float satMax = 1f, float valMin = 0f, float valMax = 1f, float alphaMin = 1f, float alphaMax = 1f)
	{
		float h = NextFloat(hueMin, hueMax);
		float s = NextFloat(satMin, satMax);
		float v = NextFloat(valMin, valMax);
		float a = NextFloat(alphaMin, alphaMax);
		return Color.HSVToRGB(h, s, v, hdr: true) * new Color(1f, 1f, 1f, a);
	}

	public float Range(float min, float max)
	{
		return NextFloat(min, max);
	}

	public int Range(int minInclusive, int maxExclusive)
	{
		if (minInclusive >= maxExclusive)
		{
			return minInclusive;
		}
		uint num = (uint)(maxExclusive - minInclusive);
		if (num <= int.MaxValue)
		{
			uint num2 = uint.MaxValue - uint.MaxValue % num;
			uint num3;
			do
			{
				num3 = NextUInt32();
			}
			while (num3 >= num2);
			return (int)(minInclusive + num3 % num);
		}
		ulong num4 = num;
		ulong num5 = (((ulong)NextUInt32() << 32) | NextUInt32()) * num4 >> 32;
		return minInclusive + (int)num5;
	}

	public float Value()
	{
		return Range(0f, 1f);
	}
}
