using System;
using System.Linq;
using UnityEngine;

namespace INab.VFXAssets;

public static class UniformMeshSamplingHelper
{
	public static Mesh RendererToMesh(Renderer meshRenderer)
	{
		if (meshRenderer is SkinnedMeshRenderer)
		{
			return (meshRenderer as SkinnedMeshRenderer).sharedMesh;
		}
		return (meshRenderer as MeshRenderer).GetComponent<MeshFilter>().sharedMesh;
	}

	public static RawMeshData ComputeDataCache(Mesh input, bool useSubMesh, int submeshIndex)
	{
		int count = 0;
		int num = 0;
		int num2;
		int vertexCount;
		if (useSubMesh)
		{
			num2 = input.GetSubMesh(submeshIndex).indexCount;
			count = input.GetSubMesh(submeshIndex).indexStart;
			vertexCount = input.GetSubMesh(submeshIndex).vertexCount;
			num = input.GetSubMesh(submeshIndex).firstVertex;
		}
		else
		{
			vertexCount = input.vertexCount;
			num2 = input.triangles.Length;
		}
		Vector3[] array = input.vertices;
		Vector3[] array2 = input.normals;
		Vector4[] array3 = input.tangents;
		Color[] array4 = input.colors;
		if (useSubMesh)
		{
			array = array.Skip(num).Take(Mathf.Min(vertexCount, array.Length - num)).ToArray();
			array2 = array2.Skip(num).Take(Mathf.Min(vertexCount, array2.Length - num)).ToArray();
			array3 = array3.Skip(num).Take(Mathf.Min(vertexCount, array3.Length - num)).ToArray();
			array4 = array4.Skip(num).Take(Mathf.Min(vertexCount, array4.Length - num)).ToArray();
		}
		array2 = ((array2.Length == vertexCount) ? array2 : null);
		array3 = ((array3.Length == vertexCount) ? array3 : null);
		array4 = ((array4.Length == vertexCount) ? array4 : null);
		RawMeshData rawMeshData = new RawMeshData();
		rawMeshData.vertices = new RawMeshData.Vertex[vertexCount];
		for (int i = 0; i < vertexCount; i++)
		{
			rawMeshData.vertices[i] = new RawMeshData.Vertex
			{
				position = array[i],
				color = ((array4 != null) ? array4[i] : Color.white),
				normal = ((array2 != null) ? array2[i] : Vector3.up),
				tangent = ((array3 != null) ? array3[i] : Vector4.one)
			};
		}
		rawMeshData.triangles = new RawMeshData.Triangle[num2 / 3];
		int[] array5 = input.triangles;
		if (useSubMesh)
		{
			array5 = array5.Skip(count).Take(num2).ToArray();
		}
		for (uint num3 = 0u; num3 < rawMeshData.triangles.Length; num3++)
		{
			rawMeshData.triangles[num3] = new RawMeshData.Triangle
			{
				a = (uint)(array5[num3 * 3] - num),
				b = (uint)(array5[num3 * 3 + 1] - num),
				c = (uint)(array5[num3 * 3 + 2] - num)
			};
		}
		if (rawMeshData.triangles.Length >= 1)
		{
			rawMeshData.accumulatedTriangleArea = new double[rawMeshData.triangles.Length];
			rawMeshData.accumulatedTriangleArea[0] = ComputeTriangleArea(rawMeshData, 0u);
			for (uint num4 = 1u; num4 < rawMeshData.triangles.Length; num4++)
			{
				rawMeshData.accumulatedTriangleArea[num4] = rawMeshData.accumulatedTriangleArea[num4 - 1] + ComputeTriangleArea(rawMeshData, num4);
			}
		}
		else
		{
			rawMeshData.accumulatedTriangleArea = new double[0];
		}
		return rawMeshData;
	}

	public static RawMeshData.Vertex GetInterpolatedVertex(RawMeshData meshData, BarycentricTriangleSampling sampling)
	{
		RawMeshData.Triangle triangle = meshData.triangles[sampling.index];
		float x = sampling.coord.x;
		float y = sampling.coord.y;
		float num = 1f - x - y;
		RawMeshData.Vertex vertex = meshData.vertices[triangle.a];
		RawMeshData.Vertex vertex2 = meshData.vertices[triangle.b];
		RawMeshData.Vertex vertex3 = meshData.vertices[triangle.c];
		RawMeshData.Vertex result = x * vertex + y * vertex2 + num * vertex3;
		result.normal = result.normal.normalized;
		Vector3 normalized = new Vector3(result.tangent.x, result.tangent.y, result.tangent.z).normalized;
		result.tangent = new Vector4(normalized.x, normalized.y, normalized.z, (result.tangent.w > 0f) ? 1f : (-1f));
		return result;
	}

	public static BarycentricTriangleSampling GetNextSampling(RawMeshData meshData, System.Random rand)
	{
		double area = rand.NextDouble() * meshData.accumulatedTriangleArea.Last();
		uint index = FindIndexOfArea(meshData, area);
		Vector2 vector = new Vector2((float)rand.NextDouble(), (float)rand.NextDouble());
		float x = vector.x;
		float num = Mathf.Sqrt(vector.y);
		float x2 = 1f - num;
		float y = (1f - x) * num;
		return new BarycentricTriangleSampling
		{
			coord = new Vector2(x2, y),
			index = index
		};
	}

	private static double ComputeTriangleArea(RawMeshData meshData, uint triangleIndex)
	{
		RawMeshData.Triangle triangle = meshData.triangles[triangleIndex];
		Vector3 position = meshData.vertices[triangle.a].position;
		Vector3 position2 = meshData.vertices[triangle.b].position;
		Vector3 position3 = meshData.vertices[triangle.c].position;
		return 0.5f * Vector3.Cross(position2 - position, position3 - position).magnitude;
	}

	private static uint FindIndexOfArea(RawMeshData meshData, double area)
	{
		uint num = 0u;
		uint num2 = (uint)(meshData.accumulatedTriangleArea.Length - 1);
		uint num3 = num2 >> 1;
		while (num2 >= num)
		{
			if (num3 > meshData.accumulatedTriangleArea.Length)
			{
				throw new InvalidOperationException("Cannot Find FindIndexOfArea");
			}
			if (meshData.accumulatedTriangleArea[num3] >= area && (num3 == 0 || meshData.accumulatedTriangleArea[num3 - 1] < area))
			{
				return num3;
			}
			if (area < meshData.accumulatedTriangleArea[num3])
			{
				num2 = num3 - 1;
			}
			else
			{
				num = num3 + 1;
			}
			num3 = num + num2 >> 1;
		}
		throw new InvalidOperationException("Cannot FindIndexOfArea");
	}
}
