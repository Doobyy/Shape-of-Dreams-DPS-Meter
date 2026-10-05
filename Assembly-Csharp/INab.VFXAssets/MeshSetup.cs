using System.Collections.Generic;
using System.Linq;
using INab.CommonVFX;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace INab.VFXAssets;

public static class MeshSetup
{
	public static string MeshProperty = "Mesh Renderer";

	public static string SkinnedMeshProperty = "Skinned Renderer";

	public static string UseSkinnedMeshProperty = "Use Skinned Mesh";

	public static void SetupPropertyBinder(VFXPropertyBinder propertyBinder, Transform transform)
	{
		IEnumerable<VFXLossyTransformBinder> propertyBinders = propertyBinder.GetPropertyBinders<VFXLossyTransformBinder>();
		VFXLossyTransformBinder vFXLossyTransformBinder = ((propertyBinders.Count() != 0) ? propertyBinders.First() : propertyBinder.AddPropertyBinder<VFXLossyTransformBinder>());
		if ((bool)transform)
		{
			vFXLossyTransformBinder.Target = transform;
		}
	}

	public static void SetupRenderer(Renderer renderer, VisualEffect visualEffect)
	{
		if (!((Object)(object)visualEffect.visualEffectAsset == null))
		{
			bool flag = false;
			if (renderer is SkinnedMeshRenderer)
			{
				visualEffect.SetSkinnedMeshRenderer(SkinnedMeshProperty, renderer as SkinnedMeshRenderer);
				flag = true;
			}
			else
			{
				MeshFilter component = renderer.GetComponent<MeshFilter>();
				visualEffect.SetMesh(MeshProperty, component.sharedMesh);
			}
			visualEffect.SetBool(UseSkinnedMeshProperty, flag);
		}
	}
}
