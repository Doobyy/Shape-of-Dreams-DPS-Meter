using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class DewPostProcessFeature : ScriptableRendererFeature
{
	private DewPostProcessPass _pass;

	public override void Create()
	{
		_pass = new DewPostProcessPass();
		_pass.material = UnityEngine.Object.Instantiate(Resources.Load<Material>("DewPostProcessingMaterial"));
		UnityEngine.Object.DontDestroyOnLoad(_pass.material);
	}

	protected override void Dispose(bool disposing)
	{
		((ScriptableRendererFeature)this).Dispose(disposing);
		if (_pass != null && _pass.material != null)
		{
			CoreUtils.Destroy((UnityEngine.Object)_pass.material);
		}
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass((ScriptableRenderPass)(object)_pass);
	}

	public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
	{
		((ScriptableRenderPass)_pass).ConfigureInput((ScriptableRenderPassInput)7);
		_pass.cameraColorTarget = renderer.cameraColorTargetHandle;
	}
}
