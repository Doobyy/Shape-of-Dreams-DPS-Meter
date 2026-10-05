using System;
using System.Reflection;
using UnityEngine;

public abstract class ModConfig
{
	[AttributeUsage(AttributeTargets.Field)]
	public class LabelTextAttribute : Attribute
	{
		public readonly string text;

		public LabelTextAttribute(string text)
		{
			this.text = text;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class DescriptionAttribute : Attribute
	{
		public readonly string text;

		public DescriptionAttribute(string text)
		{
			this.text = text;
		}
	}

	public virtual ModConfig Clone()
	{
		return (ModConfig)MemberwiseClone();
	}

	public virtual void CopyTo(ModConfig other)
	{
		FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			fieldInfo.SetValue(other, fieldInfo.GetValue(this));
		}
	}

	public virtual void BuildWidgets(Transform parent, out SafeAction onChanged, out SafeAction requestUpdate)
	{
		DewGUI.CreateWidgetsForObject(GetType(), this, parent, out onChanged, out requestUpdate);
	}
}
