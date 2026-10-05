using System;
using System.Collections.Generic;
using UnityEngine;

public class Forest_DayNight_MaterialAnimator : Forest_DayNight_Base
{
	[Serializable]
	public class TargetEntry
	{
		public Material dayMat;

		public Material nightMat;

		public MaterialProperty[] properties;

		internal Material _matInstance;
	}

	public enum PropertyType
	{
		Float,
		Color,
		Vector3
	}

	[Serializable]
	public class MaterialProperty
	{
		public PropertyType type;

		public string name;

		internal int _id;

		internal float _floatDay;

		internal float _floatNight;

		internal Color _colorDay;

		internal Color _colorNight;

		internal Vector4 _vectorDay;

		internal Vector4 _vectorNight;
	}

	public TargetEntry[] targets;

	private void Start()
	{
		if (Application.IsPlaying(this))
		{
			BuildMaterialAndPropertyIDCache();
			InstantiateAndReplaceMaterials();
			UpdateInstantiatedMaterials();
		}
	}

	private new void Update()
	{
		if (Application.IsPlaying(this) && !(Math.Abs(lastAnimatedValue - animatedValue) < 0.001f))
		{
			UpdateInstantiatedMaterials();
		}
	}

	private void OnDestroy()
	{
		DestroyCreatedMaterials();
	}

	private void UpdateInstantiatedMaterials()
	{
		if (!Application.isPlaying)
		{
			throw new InvalidOperationException();
		}
		float t = Mathf.PingPong(animatedValue, 1f);
		TargetEntry[] array = targets;
		foreach (TargetEntry targetEntry in array)
		{
			MaterialProperty[] properties = targetEntry.properties;
			foreach (MaterialProperty materialProperty in properties)
			{
				switch (materialProperty.type)
				{
				case PropertyType.Float:
					targetEntry._matInstance.SetFloat(materialProperty._id, Mathf.Lerp(materialProperty._floatDay, materialProperty._floatNight, t));
					break;
				case PropertyType.Color:
					targetEntry._matInstance.SetColor(materialProperty._id, Color.Lerp(materialProperty._colorDay, materialProperty._colorNight, t));
					break;
				case PropertyType.Vector3:
					targetEntry._matInstance.SetVector(materialProperty._id, Vector4.Lerp(materialProperty._vectorDay, materialProperty._vectorNight, t));
					break;
				}
			}
		}
	}

	private void InstantiateAndReplaceMaterials()
	{
		if (!Application.IsPlaying(this))
		{
			throw new InvalidOperationException();
		}
		Dictionary<Material, Material> dictionary = new Dictionary<Material, Material>();
		TargetEntry[] array = targets;
		foreach (TargetEntry targetEntry in array)
		{
			if (!(targetEntry.dayMat == null) && !(targetEntry.nightMat == null))
			{
				targetEntry._matInstance = UnityEngine.Object.Instantiate(targetEntry.dayMat);
				dictionary.Add(targetEntry.dayMat, targetEntry._matInstance);
				dictionary.Add(targetEntry.nightMat, targetEntry._matInstance);
			}
		}
		MeshRenderer[] array2 = UnityEngine.Object.FindObjectsOfType<MeshRenderer>(includeInactive: true);
		foreach (MeshRenderer meshRenderer in array2)
		{
			Material[] sharedMaterials = meshRenderer.sharedMaterials;
			bool flag = false;
			for (int j = 0; j < sharedMaterials.Length; j++)
			{
				if (dictionary.TryGetValue(sharedMaterials[j], out var value))
				{
					flag = true;
					sharedMaterials[j] = value;
				}
			}
			if (flag)
			{
				meshRenderer.sharedMaterials = sharedMaterials;
			}
		}
		Terrain[] array3 = UnityEngine.Object.FindObjectsOfType<Terrain>(true);
		foreach (Terrain val in array3)
		{
			if (dictionary.TryGetValue(val.materialTemplate, out var value2))
			{
				val.materialTemplate = value2;
			}
		}
		if (dictionary.TryGetValue(RenderSettings.skybox, out var value3))
		{
			RenderSettings.skybox = value3;
		}
	}

	private void BuildMaterialAndPropertyIDCache()
	{
		TargetEntry[] array = targets;
		foreach (TargetEntry targetEntry in array)
		{
			MaterialProperty[] properties = targetEntry.properties;
			foreach (MaterialProperty materialProperty in properties)
			{
				if (!targetEntry.dayMat.HasProperty(materialProperty.name))
				{
					throw new InvalidOperationException($"Property '{materialProperty.name}' not found in material '{targetEntry.dayMat}'");
				}
				materialProperty._id = Shader.PropertyToID(materialProperty.name);
			}
		}
		array = targets;
		foreach (TargetEntry targetEntry2 in array)
		{
			MaterialProperty[] properties = targetEntry2.properties;
			foreach (MaterialProperty materialProperty2 in properties)
			{
				switch (materialProperty2.type)
				{
				case PropertyType.Float:
					materialProperty2._floatDay = targetEntry2.dayMat.GetFloat(materialProperty2._id);
					materialProperty2._floatNight = targetEntry2.nightMat.GetFloat(materialProperty2._id);
					break;
				case PropertyType.Color:
					materialProperty2._colorDay = targetEntry2.dayMat.GetColor(materialProperty2._id);
					materialProperty2._colorNight = targetEntry2.nightMat.GetColor(materialProperty2._id);
					break;
				case PropertyType.Vector3:
					materialProperty2._vectorDay = targetEntry2.dayMat.GetVector(materialProperty2._id);
					materialProperty2._vectorNight = targetEntry2.nightMat.GetVector(materialProperty2._id);
					break;
				}
			}
		}
	}

	private void DestroyCreatedMaterials()
	{
		TargetEntry[] array = targets;
		foreach (TargetEntry targetEntry in array)
		{
			if (!(targetEntry._matInstance == null))
			{
				UnityEngine.Object.Destroy(targetEntry._matInstance);
			}
		}
	}
}
