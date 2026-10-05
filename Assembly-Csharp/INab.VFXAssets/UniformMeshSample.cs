using System.Collections.Generic;
using System.Linq;
using INab.CommonVFX;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace INab.VFXAssets;

[ExecuteAlways]
public abstract class UniformMeshSample : MonoBehaviour
{
	public enum EffectState
	{
		On,
		Off
	}

	[SerializeField]
	[Tooltip("Renderer of the mesh used for the effect.")]
	public Renderer meshRenderer;

	[SerializeField]
	[Tooltip("Instance of the VFX Uniform Mesh Baker.")]
	public UniformMeshBaker meshBaker = new UniformMeshBaker();

	[Range(0.01f, 10f)]
	[SerializeField]
	[Tooltip("Muleffectly sample count by this value to control density of the particles. Keep this as low as possible.")]
	public float sampleCountMultiplier = 1f;

	[SerializeField]
	[Tooltip("Whether to use the effect only on one submesh.")]
	public bool usePerSubmeshBaking;

	[SerializeField]
	[Tooltip("Submesh index.")]
	public int submeshIndex;

	[Tooltip("Effect prefab game object.")]
	public GameObject effectPrefab;

	[Tooltip("Instantiated effect prefab game object.")]
	public GameObject instantiatedEffectPrefab;

	[Tooltip("Enable debug mode for detailed logs.")]
	public bool debugMode;

	[Tooltip("User defined name for the effect.")]
	public string effectName = "Effect 1";

	public VisualEffect vfxComponent;

	public VFXPropertyBinder vfxBinder;

	public EffectState currentEffectState = EffectState.Off;

	[HideInInspector]
	public bool _Foldout_1 = true;

	[HideInInspector]
	public bool _Foldout_2 = true;

	[HideInInspector]
	public bool _Foldout_3 = true;

	[HideInInspector]
	public bool _Foldout_4 = true;

	[HideInInspector]
	public bool _Foldout_5 = true;

	public abstract string DefaultPrefabPath { get; }

	public Transform meshTransform
	{
		get
		{
			if (meshRenderer != null)
			{
				if (IsSkinnedMesh)
				{
					return (meshRenderer as SkinnedMeshRenderer).transform;
				}
				return meshRenderer.transform;
			}
			return null;
		}
	}

	public bool IsSkinnedMesh
	{
		get
		{
			return meshRenderer is SkinnedMeshRenderer;
		}
		private set
		{
		}
	}

	public bool IsMeshReadable
	{
		get
		{
			if (meshRenderer == null)
			{
				return true;
			}
			if (IsSkinnedMesh)
			{
				return (meshRenderer as SkinnedMeshRenderer).sharedMesh.isReadable;
			}
			return meshRenderer.GetComponent<MeshFilter>().sharedMesh.isReadable;
		}
		private set
		{
		}
	}

	public string PrefabName
	{
		get
		{
			if (!effectPrefab)
			{
				return "None";
			}
			return effectPrefab.name;
		}
	}

	public string PrefabAssetPath => "None";

	public bool _SaveAsNewPrefab()
	{
		return false;
	}

	public void _ApplyPrefabChanges()
	{
	}

	public bool _LoadPrefab()
	{
		return false;
	}

	public bool _InstantiateEffectPrefab(bool autoStartEffect = true)
	{
		if (instantiatedEffectPrefab != null)
		{
			Object.Destroy(instantiatedEffectPrefab);
			instantiatedEffectPrefab = null;
		}
		if (effectPrefab != null)
		{
			GameObject gameObject = Object.Instantiate(effectPrefab, base.transform);
			if (gameObject != null)
			{
				Transform transform = gameObject.transform;
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
				transform.localScale = Vector3.one;
				gameObject.name = effectPrefab.name + " (Instance) [" + effectName + "]";
				instantiatedEffectPrefab = gameObject;
				vfxComponent = gameObject.GetComponent<VisualEffect>();
				vfxBinder = gameObject.GetComponent<VFXPropertyBinder>();
				ConfigureVFXBinders();
			}
		}
		SetupVfxGraph();
		if (autoStartEffect)
		{
			StartEffect();
		}
		return true;
	}

	public void _BakeUniformMesh()
	{
		if ((bool)meshRenderer && (bool)(Object)(object)vfxComponent)
		{
			meshBaker.Bake(vfxComponent, meshRenderer);
		}
	}

	public void _SetGraphicsBuffer()
	{
		if ((bool)(Object)(object)vfxComponent)
		{
			meshBaker.SetGraphicsBuffer(vfxComponent);
		}
	}

	public void _FindRenderer()
	{
		meshRenderer = GetComponentInChildren<Renderer>();
		if (meshRenderer == null)
		{
			if ((bool)gameObject.transform.parent)
			{
				meshRenderer = gameObject.transform.parent.GetComponentInChildren<Renderer>();
			}
			else
			{
				meshRenderer = gameObject.GetComponentInParent<Renderer>();
			}
		}
		if (meshRenderer == null)
		{
			Debug.LogWarning("No renderer could be found.");
		}
		if (!(meshRenderer is SkinnedMeshRenderer) && !(meshRenderer is MeshRenderer))
		{
			meshRenderer = null;
			Debug.LogWarning("Found renderer different than SkinnedMeshRenderer or MeshRenderer.");
		}
	}

	public void ConfigureVFXBinders()
	{
		if (!((Object)(object)vfxBinder == null))
		{
			List<VFXLossyTransformBinder> list = vfxBinder.GetPropertyBinders<VFXLossyTransformBinder>().ToList();
			while (list.Count < 1)
			{
				list.Add(vfxBinder.AddPropertyBinder<VFXLossyTransformBinder>());
			}
			list[0].Target = meshTransform;
			list[0].Property = "Transform";
		}
	}

	public void SetProperty_EffectActive(bool isActive)
	{
		if ((bool)(Object)(object)vfxComponent && vfxComponent.HasBool("Effect Active"))
		{
			vfxComponent.SetBool("Effect Active", isActive);
		}
	}

	public void SendPlayEvent()
	{
		VisualEffect val = vfxComponent;
		if (val != null)
		{
			val.Play();
		}
	}

	public void SendStopEvent()
	{
		VisualEffect val = vfxComponent;
		if (val != null)
		{
			val.Stop();
		}
	}

	private void OnEnable()
	{
		bool flag = false;
		if ((Object)(object)vfxBinder == null)
		{
			return;
		}
		foreach (VFXLossyTransformBinder propertyBinder in vfxBinder.GetPropertyBinders<VFXLossyTransformBinder>())
		{
			if (propertyBinder.Target == null)
			{
				flag = true;
			}
		}
		if (flag)
		{
			ConfigureVFXBinders();
		}
		SetupVfxGraph();
	}

	protected virtual void Start()
	{
		SetupVfxGraph();
	}

	protected virtual void Update()
	{
		if ((bool)(Object)(object)vfxComponent && (bool)meshRenderer)
		{
			meshBaker.Update(vfxComponent, meshRenderer);
		}
	}

	protected virtual void OnDisable()
	{
		meshBaker.OnDisable();
	}

	protected virtual void OnValidate()
	{
		if (enabled && gameObject.activeSelf)
		{
			meshBaker.SampleCountMultiplier = sampleCountMultiplier;
			meshBaker.UsePerSubmeshBaking = usePerSubmeshBaking;
			meshBaker.SubmeshIndex = submeshIndex;
			_SetGraphicsBuffer();
		}
	}

	public void SetNewEffectPrefab(GameObject newEffectPrefab)
	{
		StopEffect();
		effectPrefab = newEffectPrefab;
		_InstantiateEffectPrefab(autoStartEffect: false);
	}

	public void StartEffect()
	{
		SendPlayEvent();
		SetProperty_EffectActive(isActive: true);
		currentEffectState = EffectState.On;
	}

	public void StopEffect()
	{
		SetProperty_EffectActive(isActive: false);
		SendStopEvent();
		currentEffectState = EffectState.Off;
	}

	public virtual void SetupVfxGraph()
	{
		if ((bool)meshRenderer)
		{
			MeshSetup.SetupRenderer(meshRenderer, vfxComponent);
		}
		OnValidate();
		_BakeUniformMesh();
		if ((bool)(Object)(object)vfxComponent)
		{
			vfxComponent.Reinit();
		}
	}
}
