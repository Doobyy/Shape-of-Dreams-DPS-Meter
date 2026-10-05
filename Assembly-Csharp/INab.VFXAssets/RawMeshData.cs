using UnityEngine;

namespace INab.VFXAssets;

public class RawMeshData
{
	public struct Vertex
	{
		public Vector3 position;

		public Color color;

		public Vector3 normal;

		public Vector4 tangent;

		public static Vertex operator +(Vertex a, Vertex b)
		{
			return new Vertex
			{
				position = a.position + b.position,
				color = a.color + b.color,
				normal = a.normal + b.normal,
				tangent = a.tangent + b.tangent
			};
		}

		public static Vertex operator *(float a, Vertex b)
		{
			return new Vertex
			{
				position = a * b.position,
				color = a * b.color,
				normal = a * b.normal,
				tangent = a * b.tangent
			};
		}
	}

	public struct Triangle
	{
		public uint a;

		public uint b;

		public uint c;
	}

	public Vertex[] vertices;

	public Triangle[] triangles;

	public double[] accumulatedTriangleArea;
}
