using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace INab.CommonVFX;

[VFXBinder("Transform/Lossy Transform")]
public class VFXLossyTransformBinder : VFXBinderBase
{
	[VFXPropertyBinding(new string[] { "Lossy Transform" })]
	[SerializeField]
	[FormerlySerializedAs("m_Parameter")]
	protected ExposedProperty m_Property = ExposedProperty.op_Implicit("Transform");

	public Transform Target;

	private ExposedProperty Position;

	private ExposedProperty Angles;

	private ExposedProperty Scale;

	public string Property
	{
		get
		{
			return (string)m_Property;
		}
		set
		{
			m_Property = ExposedProperty.op_Implicit(value);
			UpdateSubProperties();
		}
	}

	protected override void OnEnable()
	{
		((VFXBinderBase)this).OnEnable();
		UpdateSubProperties();
	}

	private void OnValidate()
	{
		UpdateSubProperties();
	}

	private void UpdateSubProperties()
	{
		Position = m_Property + ExposedProperty.op_Implicit("_position");
		Angles = m_Property + ExposedProperty.op_Implicit("_angles");
		Scale = m_Property + ExposedProperty.op_Implicit("_scale");
	}

	public override bool IsValid(VisualEffect component)
	{
		if (Target != null && component.HasVector3(ExposedProperty.op_Implicit(Position)) && component.HasVector3(ExposedProperty.op_Implicit(Angles)))
		{
			return component.HasVector3(ExposedProperty.op_Implicit(Scale));
		}
		return false;
	}

	public override void UpdateBinding(VisualEffect component)
	{
		component.SetVector3(ExposedProperty.op_Implicit(Position), Target.position);
		component.SetVector3(ExposedProperty.op_Implicit(Angles), Target.eulerAngles);
		component.SetVector3(ExposedProperty.op_Implicit(Scale), Target.lossyScale);
	}

	public override string ToString()
	{
		return string.Format("Lossy Scale Transform : '{0}' -> {1}", m_Property, (Target == null) ? "(null)" : Target.name);
	}
}
