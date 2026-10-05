using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;

namespace INab.VFXAssets;

[Serializable]
public class UniformMeshBaker
{
	public static string GraphicsBufferName = "UniformMeshBuffer";

	[Tooltip("Amount of points from which particles would be spawned.")]
	[SerializeField]
	private int _sampleCount = 2048;

	[Range(0.01f, 10f)]
	[SerializeField]
	[Tooltip("Multiply sample count by this value to control density of the particles. Keep this as low as possible.")]
	public float SampleCountMultiplier = 1f;

	[SerializeField]
	public BarycentricTriangleSampling[] m_BakedSampling;

	public GraphicsBuffer m_Buffer;

	[SerializeField]
	public bool UsePerSubmeshBaking;

	[SerializeField]
	public int SubmeshIndex;

	public int SampleCount => Mathf.Min((int)((float)_sampleCount * Mathf.Pow(SampleCountMultiplier, 2f)), 100000);

	private void ComputeBakedSampling(VisualEffect visualEffect, Mesh mesh)
	{
		if ((UnityEngine.Object)(object)visualEffect == null)
		{
			Debug.LogWarning("UniformBaker expects a VisualEffect on the shared game object.");
		}
		else
		{
			if (!visualEffect.HasGraphicsBuffer(GraphicsBufferName))
			{
				return;
			}
			RawMeshData rawMeshData = UniformMeshSamplingHelper.ComputeDataCache(mesh, UsePerSubmeshBaking, SubmeshIndex);
			if (UsePerSubmeshBaking)
			{
				SubMeshDescriptor subMesh = mesh.GetSubMesh(SubmeshIndex);
				_sampleCount = subMesh.indexCount / 3;
				if (visualEffect.HasUInt("Start Triangle Index"))
				{
					visualEffect.SetUInt("Start Triangle Index", (uint)subMesh.indexStart / 3u);
				}
			}
			else
			{
				_sampleCount = rawMeshData.triangles.Length;
				if (visualEffect.HasUInt("Start Triangle Index"))
				{
					visualEffect.SetUInt("Start Triangle Index", 0u);
				}
			}
			System.Random rand = new System.Random(123);
			m_BakedSampling = new BarycentricTriangleSampling[SampleCount];
			for (int i = 0; i < SampleCount; i++)
			{
				m_BakedSampling[i] = UniformMeshSamplingHelper.GetNextSampling(rawMeshData, rand);
			}
		}
	}

	private void UpdateGraphicsBuffer()
	{
		if (m_BakedSampling != null && SampleCount == m_BakedSampling.Length)
		{
			if (m_Buffer != null)
			{
				m_Buffer.Release();
				m_Buffer = null;
			}
			m_Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, SampleCount, Marshal.SizeOf(typeof(BarycentricTriangleSampling)));
			m_Buffer.SetData(m_BakedSampling);
		}
	}

	private void BindGraphicsBuffer(VisualEffect vfx)
	{
		if (!((UnityEngine.Object)(object)vfx == null) && vfx.HasGraphicsBuffer(GraphicsBufferName))
		{
			vfx.SetGraphicsBuffer(GraphicsBufferName, m_Buffer);
		}
	}

	public void Update(VisualEffect visualEffect, Renderer renderer)
	{
		if (m_BakedSampling == null || m_BakedSampling.Length < 1)
		{
			Bake(visualEffect, renderer);
		}
		else if (m_Buffer == null)
		{
			Bake(visualEffect, renderer);
		}
	}

	public void OnDisable()
	{
		if (m_Buffer != null)
		{
			m_Buffer.Release();
			m_Buffer = null;
		}
	}

	public void Bake(VisualEffect visualEffect, Renderer renderer)
	{
		ComputeBakedSampling(visualEffect, UniformMeshSamplingHelper.RendererToMesh(renderer));
		UpdateGraphicsBuffer();
		BindGraphicsBuffer(visualEffect);
	}

	public void SetGraphicsBuffer(VisualEffect visualEffect)
	{
		UpdateGraphicsBuffer();
		BindGraphicsBuffer(visualEffect);
	}
}
