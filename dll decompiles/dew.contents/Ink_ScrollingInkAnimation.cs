using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[LogicUpdatePriority(1600)]
public class Ink_ScrollingInkAnimation : LogicBehaviour
{
	private struct InkMaterailItem
	{
		public Material targetMat;

		public Material ogMat;

		public float[] noiseWeights;

		public Vector4[] noiseSTs;

		public Vector4[] normalSTs;

		public Vector4[] noiseScales;
	}

	private static readonly int L1NoiseWeight = Shader.PropertyToID("_L1NoiseWeight");

	private static readonly int L2NoiseWeight = Shader.PropertyToID("_L2NoiseWeight");

	private static readonly int L3NoiseWeight = Shader.PropertyToID("_L3NoiseWeight");

	private static readonly int L4NoiseWeight = Shader.PropertyToID("_L4NoiseWeight");

	private static readonly int L5NoiseWeight = Shader.PropertyToID("_L5NoiseWeight");

	private static readonly int L6NoiseWeight = Shader.PropertyToID("_L6NoiseWeight");

	private static readonly int L7NoiseWeight = Shader.PropertyToID("_L7NoiseWeight");

	private static readonly int[] NoiseWeight = new int[7] { L1NoiseWeight, L2NoiseWeight, L3NoiseWeight, L4NoiseWeight, L5NoiseWeight, L6NoiseWeight, L7NoiseWeight };

	private static readonly int L0Noise_ST = Shader.PropertyToID("_L0Noise_ST");

	private static readonly int L1Noise_ST = Shader.PropertyToID("_L1Noise_ST");

	private static readonly int L2Noise_ST = Shader.PropertyToID("_L2Noise_ST");

	private static readonly int L3Noise_ST = Shader.PropertyToID("_L3Noise_ST");

	private static readonly int L4Noise_ST = Shader.PropertyToID("_L4Noise_ST");

	private static readonly int L5Noise_ST = Shader.PropertyToID("_L5Noise_ST");

	private static readonly int L6Noise_ST = Shader.PropertyToID("_L6Noise_ST");

	private static readonly int L7Noise_ST = Shader.PropertyToID("_L7Noise_ST");

	private static readonly int[] NoiseST = new int[8] { L0Noise_ST, L1Noise_ST, L2Noise_ST, L3Noise_ST, L4Noise_ST, L5Noise_ST, L6Noise_ST, L7Noise_ST };

	private static readonly int L0Normal_ST = Shader.PropertyToID("_L0Normal_ST");

	private static readonly int L1Normal_ST = Shader.PropertyToID("_L1Normal_ST");

	private static readonly int L2Normal_ST = Shader.PropertyToID("_L2Normal_ST");

	private static readonly int L3Normal_ST = Shader.PropertyToID("_L3Normal_ST");

	private static readonly int L4Normal_ST = Shader.PropertyToID("_L4Normal_ST");

	private static readonly int L5Normal_ST = Shader.PropertyToID("_L5Normal_ST");

	private static readonly int L6Normal_ST = Shader.PropertyToID("_L6Normal_ST");

	private static readonly int L7Normal_ST = Shader.PropertyToID("_L7Normal_ST");

	private static readonly int[] NormalST = new int[8] { L0Normal_ST, L1Normal_ST, L2Normal_ST, L3Normal_ST, L4Normal_ST, L5Normal_ST, L6Normal_ST, L7Normal_ST };

	private static readonly int L0NoiseScales = Shader.PropertyToID("_L0NoiseScales");

	private static readonly int L1NoiseScales = Shader.PropertyToID("_L1NoiseScales");

	private static readonly int L2NoiseScales = Shader.PropertyToID("_L2NoiseScales");

	private static readonly int L3NoiseScales = Shader.PropertyToID("_L3NoiseScales");

	private static readonly int L4NoiseScales = Shader.PropertyToID("_L4NoiseScales");

	private static readonly int L5NoiseScales = Shader.PropertyToID("_L5NoiseScales");

	private static readonly int L6NoiseScales = Shader.PropertyToID("_L6NoiseScales");

	private static readonly int L7NoiseScales = Shader.PropertyToID("_L7NoiseScales");

	private static readonly int[] NoiseScales = new int[8] { L0NoiseScales, L1NoiseScales, L2NoiseScales, L3NoiseScales, L4NoiseScales, L5NoiseScales, L6NoiseScales, L7NoiseScales };

	public bool doTerrain;

	public bool l1NoiseWeight;

	public Vector2 l1SpeedAndMagnitude = new Vector2(1.25f, 0.5f);

	[Space(20f)]
	public bool l2NoiseWeight;

	public Vector2 l2SpeedAndMagnitude = new Vector2(1.25f, 0.5f);

	[Space(20f)]
	public bool l3NoiseWeight;

	public Vector2 l3SpeedAndMagnitude = new Vector2(1.25f, 0.5f);

	[Space(20f)]
	public bool l4NoiseWeight;

	public Vector2 l4SpeedAndMagnitude = new Vector2(1.25f, 0.5f);

	[Space(20f)]
	public bool l5NoiseWeight;

	public Vector2 l5SpeedAndMagnitude = new Vector2(1.25f, 0.5f);

	[Space(20f)]
	public bool l6NoiseWeight;

	public Vector2 l6SpeedAndMagnitude = new Vector2(1.25f, 0.5f);

	[Space(20f)]
	public bool l7NoiseWeight;

	public Vector2 l7SpeedAndMagnitude = new Vector2(1.25f, 0.5f);

	[Space(20f)]
	public bool l0NoiseScroll;

	public Vector2 l0ScrollSpeed;

	public bool l0NormalScroll;

	[Space(20f)]
	public bool l1NoiseScroll;

	public Vector2 l1ScrollSpeed;

	public bool l1NormalScroll;

	[Space(20f)]
	public bool l2NoiseScroll;

	public Vector2 l2ScrollSpeed;

	public bool l2NormalScroll;

	[Space(20f)]
	public bool l3NoiseScroll;

	public Vector2 l3ScrollSpeed;

	public bool l3NormalScroll;

	[Space(20f)]
	public bool l4NoiseScroll;

	public Vector2 l4ScrollSpeed;

	public bool l4NormalScroll;

	[Space(20f)]
	public bool l5NoiseScroll;

	public Vector2 l5ScrollSpeed;

	public bool l5NormalScroll;

	[Space(20f)]
	public bool l6NoiseScroll;

	public Vector2 l6ScrollSpeed;

	public bool l6NormalScroll;

	[Space(20f)]
	public bool l7NoiseScroll;

	public Vector2 l7ScrollSpeed;

	public bool l7NormalScroll;

	[Space(20f)]
	public bool l0DetailScalePulse;

	public Vector2 l0DetailPulseParams = new Vector2(0.5f, 0.5f);

	[Space(20f)]
	public bool l1DetailScalePulse;

	public Vector2 l1DetailPulseParams = new Vector2(0.5f, 0.5f);

	[Space(20f)]
	public bool l2DetailScalePulse;

	public Vector2 l2DetailPulseParams = new Vector2(0.5f, 0.5f);

	[Space(20f)]
	public bool l3DetailScalePulse;

	public Vector2 l3DetailPulseParams = new Vector2(0.5f, 0.5f);

	[Space(20f)]
	public bool l4DetailScalePulse;

	public Vector2 l4DetailPulseParams = new Vector2(0.5f, 0.5f);

	[Space(20f)]
	public bool l5DetailScalePulse;

	public Vector2 l5DetailPulseParams = new Vector2(0.5f, 0.5f);

	[Space(20f)]
	public bool l6DetailScalePulse;

	public Vector2 l6DetailPulseParams = new Vector2(0.5f, 0.5f);

	[Space(20f)]
	public bool l7DetailScalePulse;

	public Vector2 l7DetailPulseParams = new Vector2(0.5f, 0.5f);

	private List<InkMaterailItem> _targets = new List<InkMaterailItem>();

	private void Start()
	{
		List<GameObject> list = DewPool.GetList(out ListReturnHandle<GameObject> handle);
		SceneManager.GetActiveScene().GetRootGameObjects(list);
		foreach (GameObject item3 in list)
		{
			ListReturnHandle<Renderer> handle2;
			foreach (Renderer item4 in item3.GetComponentsInChildrenNonAlloc(out handle2))
			{
				Material[] sharedMaterials = item4.sharedMaterials;
				for (int i = 0; i < sharedMaterials.Length; i++)
				{
					if (sharedMaterials[i] == null || sharedMaterials[i].shader == null)
					{
						continue;
					}
					string text = sharedMaterials[i].shader.name;
					if (!(text != "Dew/Dew New Terrain") || !(text != "Dew/Dew New Ground"))
					{
						Material m = sharedMaterials[i];
						if (_targets.FindIndex((InkMaterailItem prev) => prev.ogMat == m) < 0)
						{
							InkMaterailItem item = CreateItem(m);
							_targets.Add(item);
							sharedMaterials[i] = item.targetMat;
						}
					}
				}
				item4.sharedMaterials = sharedMaterials;
			}
			handle2.Return();
		}
		handle.Return();
		if (!doTerrain)
		{
			return;
		}
		Terrain[] array = Object.FindObjectsOfType<Terrain>();
		foreach (Terrain t in array)
		{
			if (!((Object)(object)t == null) && _targets.FindIndex((InkMaterailItem prev) => prev.ogMat == t.materialTemplate) < 0)
			{
				InkMaterailItem item2 = CreateItem(t.materialTemplate);
				_targets.Add(item2);
				t.materialTemplate = item2.targetMat;
			}
		}
		static InkMaterailItem CreateItem(Material material)
		{
			InkMaterailItem result = new InkMaterailItem
			{
				ogMat = material,
				targetMat = Object.Instantiate(material),
				noiseWeights = new float[7],
				noiseSTs = new Vector4[8],
				normalSTs = new Vector4[8],
				noiseScales = new Vector4[8]
			};
			for (int j = 0; j < 7; j++)
			{
				result.noiseWeights[j] = material.GetFloat(NoiseWeight[j]);
			}
			for (int k = 0; k < 8; k++)
			{
				result.noiseSTs[k] = material.GetVector(NoiseST[k]);
				result.normalSTs[k] = material.GetVector(NormalST[k]);
				result.noiseScales[k] = material.GetVector(NoiseScales[k]);
			}
			return result;
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (!GraphicsManager.WasLowFpsInLast5Seconds() && Time.frameCount % 3 == 0)
		{
			int t;
			for (t = 0; t < _targets.Count; t++)
			{
				Material m = _targets[t].targetMat;
				AnimateNoiseWeight(l1NoiseWeight, 0, l1SpeedAndMagnitude.x, l1SpeedAndMagnitude.y);
				AnimateNoiseWeight(l2NoiseWeight, 1, l2SpeedAndMagnitude.x, l2SpeedAndMagnitude.y);
				AnimateNoiseWeight(l3NoiseWeight, 2, l3SpeedAndMagnitude.x, l3SpeedAndMagnitude.y);
				AnimateNoiseWeight(l4NoiseWeight, 3, l4SpeedAndMagnitude.x, l4SpeedAndMagnitude.y);
				AnimateNoiseWeight(l5NoiseWeight, 4, l5SpeedAndMagnitude.x, l5SpeedAndMagnitude.y);
				AnimateNoiseWeight(l6NoiseWeight, 5, l6SpeedAndMagnitude.x, l6SpeedAndMagnitude.y);
				AnimateNoiseWeight(l7NoiseWeight, 6, l7SpeedAndMagnitude.x, l7SpeedAndMagnitude.y);
				AnimateNoiseScroll(l0NoiseScroll, l0NormalScroll, 0, l0ScrollSpeed);
				AnimateNoiseScroll(l1NoiseScroll, l1NormalScroll, 1, l1ScrollSpeed);
				AnimateNoiseScroll(l2NoiseScroll, l2NormalScroll, 2, l2ScrollSpeed);
				AnimateNoiseScroll(l3NoiseScroll, l3NormalScroll, 3, l3ScrollSpeed);
				AnimateNoiseScroll(l4NoiseScroll, l4NormalScroll, 4, l4ScrollSpeed);
				AnimateNoiseScroll(l5NoiseScroll, l5NormalScroll, 5, l5ScrollSpeed);
				AnimateNoiseScroll(l6NoiseScroll, l6NormalScroll, 6, l6ScrollSpeed);
				AnimateNoiseScroll(l7NoiseScroll, l7NormalScroll, 7, l7ScrollSpeed);
				AnimateDetailScale(l0DetailScalePulse, 0, l0DetailPulseParams.x, l0DetailPulseParams.y);
				AnimateDetailScale(l1DetailScalePulse, 1, l1DetailPulseParams.x, l1DetailPulseParams.y);
				AnimateDetailScale(l2DetailScalePulse, 2, l2DetailPulseParams.x, l2DetailPulseParams.y);
				AnimateDetailScale(l3DetailScalePulse, 3, l3DetailPulseParams.x, l3DetailPulseParams.y);
				AnimateDetailScale(l4DetailScalePulse, 4, l4DetailPulseParams.x, l4DetailPulseParams.y);
				AnimateDetailScale(l5DetailScalePulse, 5, l5DetailPulseParams.x, l5DetailPulseParams.y);
				AnimateDetailScale(l6DetailScalePulse, 6, l6DetailPulseParams.x, l6DetailPulseParams.y);
				AnimateDetailScale(l7DetailScalePulse, 7, l7DetailPulseParams.x, l7DetailPulseParams.y);
				void AnimateDetailScale(bool value, int index, float speed, float magnitude)
				{
					if (value)
					{
						float num = Time.time + (float)index * 4.73f + 2.1f;
						Vector4 vector = _targets[t].noiseScales[index];
						float num2 = Mathf.Sin(num * speed) * magnitude;
						Vector4 value2 = new Vector4(vector.x, vector.y, vector.z + num2, vector.w);
						m.SetVector(NoiseScales[index], value2);
						Vector4 vector2 = _targets[t].noiseSTs[index];
						float num3 = (vector.z + num2) / vector.z;
						float x = vector2.x;
						float num4 = 0.5f / x * (1f - num3);
						Vector4 value3 = new Vector4(vector2.x, vector2.y, vector2.z + num4, vector2.w);
						m.SetVector(NoiseST[index], value3);
					}
				}
				void AnimateNoiseScroll(bool value, bool normalEnable, int index, Vector2 speed)
				{
					if (value)
					{
						float num = Time.time + (float)index * 8.051f + 4.7f;
						Vector4 value2 = _targets[t].noiseSTs[index];
						value2.z += speed.x * num;
						value2.w += speed.y * num;
						m.SetVector(NoiseST[index], value2);
						if (normalEnable)
						{
							Vector4 value3 = _targets[t].normalSTs[index];
							value3.z += speed.x * num;
							value3.w += speed.y * num;
							m.SetVector(NormalST[index], value3);
						}
					}
				}
				void AnimateNoiseWeight(bool value, int index, float speed, float magnitude)
				{
					if (value)
					{
						float num = Time.time + (float)index * 12.152f;
						m.SetFloat(NoiseWeight[index], _targets[t].noiseWeights[index] + Mathf.Sin(num * speed) * magnitude);
					}
				}
			}
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _targets.Count; i++)
		{
			if (!(_targets[i].targetMat == null))
			{
				Object.Destroy(_targets[i].targetMat);
			}
		}
		_targets.Clear();
	}
}
