using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class DewPostProcessPass : ScriptableRenderPass
{
	public RTHandle cameraColorTarget;

	public Material material;

	private ProfilingSampler _profilingSampler = new ProfilingSampler("DewPostProcess");

	private static readonly int _VFIntensity = Shader.PropertyToID("_VFIntensity");

	private static readonly int _VFColor = Shader.PropertyToID("_VFColor");

	private static readonly int _VFRange = Shader.PropertyToID("_VFRange");

	private static readonly int _VFIntensityBottom = Shader.PropertyToID("_VFIntensityBottom");

	private static readonly int _VFRangeBottom = Shader.PropertyToID("_VFRangeBottom");

	private static readonly int _VFNoise = Shader.PropertyToID("_VFNoise");

	private static readonly int _VFNoiseScale = Shader.PropertyToID("_VFNoiseScale");

	private static readonly int _VFNoiseWeight = Shader.PropertyToID("_VFNoiseWeight");

	private static readonly int _VFNoiseScroll = Shader.PropertyToID("_VFNoiseScroll");

	private static readonly int _VFSteps = Shader.PropertyToID("_VFSteps");

	private static readonly int _ThicknessMultiplier = Shader.PropertyToID("_ThicknessMultiplier");

	private static readonly int _OutlineMultiplier = Shader.PropertyToID("_OutlineMultiplier");

	private static readonly int _HighlightedBoost = Shader.PropertyToID("_HighlightedBoost");

	private static readonly int _CartoonOutline = Shader.PropertyToID("_CartoonOutline");

	private static readonly int _ViewProjectInverse = Shader.PropertyToID("_ViewProjectInverse");

	public DewPostProcessPass()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected Obj, but got Unknown
		((ScriptableRenderPass)this).renderPassEvent = (RenderPassEvent)450;
	}

	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		((ScriptableRenderPass)this).ConfigureTarget(cameraColorTarget);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		CameraType cameraType = renderingData.cameraData.cameraType;
		if (cameraType != CameraType.Game && cameraType != CameraType.SceneView)
		{
			return;
		}
		DewPostProcessingComponent component = VolumeManager.instance.stack.GetComponent<DewPostProcessingComponent>();
		if (!((UnityEngine.Object)(object)component == null) && component.IsActive())
		{
			if (material == null)
			{
				material = UnityEngine.Object.Instantiate(Resources.Load<Material>("DewPostProcessingMaterial"));
			}
			CommandBuffer commandBuffer = CommandBufferPool.Get("Dew Post Processing");
			ProfilingScope val = new ProfilingScope(commandBuffer, _profilingSampler);
			try
			{
				material.SetFloat(_VFIntensity, ((VolumeParameter<float>)(object)component.vfIntensity).value);
				material.SetColor(_VFColor, ((VolumeParameter<Color>)(object)component.vfColor).value);
				material.SetVector(_VFRange, ((VolumeParameter<Vector2>)(object)component.vfRange).value);
				material.SetFloat(_VFIntensityBottom, ((VolumeParameter<float>)(object)component.vfIntensityBottom).value);
				material.SetVector(_VFRangeBottom, ((VolumeParameter<Vector2>)(object)component.vfRangeBottom).value);
				material.SetTexture(_VFNoise, ((VolumeParameter<Texture>)(object)component.vfNoise).value);
				material.SetVector(_VFNoiseScale, ((VolumeParameter<Vector2>)(object)component.vfNoiseScale).value);
				material.SetFloat(_VFNoiseWeight, ((VolumeParameter<float>)(object)component.vfNoiseWeight).value);
				material.SetVector(_VFNoiseScroll, ((VolumeParameter<Vector2>)(object)component.vfNoiseScroll).value);
				material.SetInt(_VFSteps, ((VolumeParameter<int>)(object)component.vfSteps).value);
				material.SetFloat(_ThicknessMultiplier, ((VolumeParameter<float>)(object)component.sobelThickness).value);
				material.SetFloat(_OutlineMultiplier, ((VolumeParameter<float>)(object)component.sobelMultiplier).value);
				material.SetFloat(_HighlightedBoost, ((VolumeParameter<float>)(object)component.highlightedBoost).value);
				material.SetFloat(_CartoonOutline, ((VolumeParameter<float>)(object)component.cartoonOutline).value);
				Camera camera = renderingData.cameraData.camera;
				material.SetMatrix(_ViewProjectInverse, (camera.projectionMatrix * camera.worldToCameraMatrix).inverse);
				Blitter.BlitCameraTexture(commandBuffer, cameraColorTarget, cameraColorTarget, material, 0);
			}
			finally
			{
				((IDisposable)val/*cast due to constrained. prefix*/).Dispose();
			}
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			CommandBufferPool.Release(commandBuffer);
		}
	}
}
