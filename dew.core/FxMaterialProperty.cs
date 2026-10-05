using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FxMaterialProperty : FxInterpolatedEffectBase
{
	public enum PropertyType
	{
		Float,
		Color
	}

	[Serializable]
	public struct PropertyEntry
	{
		public PropertyType type;

		public string property;

		public Renderer target;

		public Graphic targetUi;

		public AnimationCurve valueCurve;

		[ColorUsage(true, true)]
		public Color startColor;

		[ColorUsage(true, true)]
		public Color endColor;

		internal int _propertyId;

		internal List<Material> _instance;
	}

	public PropertyEntry[] properties;

	private Dictionary<Material, Material> _replacements;

	protected override void OnInit()
	{
		base.OnInit();
		_replacements = new Dictionary<Material, Material>();
		for (int i = 0; i < properties.Length; i++)
		{
			PropertyEntry propertyEntry = properties[i];
			propertyEntry._propertyId = Shader.PropertyToID(propertyEntry.property);
			Material[] array = ((propertyEntry.target != null) ? propertyEntry.target.sharedMaterials : new Material[1] { propertyEntry.targetUi.material });
			Material[] array2 = new Material[array.Length];
			propertyEntry._instance = new List<Material>();
			for (int j = 0; j < array.Length; j++)
			{
				Material material = array[j];
				if (_replacements.TryGetValue(material, out var value))
				{
					array2[j] = value;
					propertyEntry._instance.Add(value);
					continue;
				}
				Material material2 = UnityEngine.Object.Instantiate(material);
				_replacements.Add(material, material2);
				array2[j] = material2;
				propertyEntry._instance.Add(material2);
				properties[i] = propertyEntry;
			}
			if (propertyEntry.target != null)
			{
				propertyEntry.target.sharedMaterials = array2;
			}
			else
			{
				propertyEntry.targetUi.material = array2[0];
			}
		}
	}

	protected override void ValueSetter(float value)
	{
		if (_replacements == null)
		{
			return;
		}
		PropertyEntry[] array = properties;
		for (int i = 0; i < array.Length; i++)
		{
			PropertyEntry propertyEntry = array[i];
			if (propertyEntry._instance == null || propertyEntry._instance.Count < 0)
			{
				continue;
			}
			foreach (Material item in propertyEntry._instance)
			{
				switch (propertyEntry.type)
				{
				case PropertyType.Float:
					item.SetFloat(propertyEntry._propertyId, propertyEntry.valueCurve.Evaluate(value));
					break;
				case PropertyType.Color:
					item.SetColor(propertyEntry._propertyId, Color.Lerp(propertyEntry.startColor, propertyEntry.endColor, propertyEntry.valueCurve.Evaluate(value)));
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}
	}

	private void OnDestroy()
	{
		ClearMaterials();
	}

	private void ClearMaterials()
	{
		if (_replacements == null)
		{
			return;
		}
		foreach (KeyValuePair<Material, Material> replacement in _replacements)
		{
			if (!(replacement.Value == null))
			{
				UnityEngine.Object.Destroy(replacement.Value);
			}
		}
		_replacements = null;
	}
}
