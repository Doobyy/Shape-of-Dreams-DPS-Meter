using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class DewGBufferClearFeature : ScriptableRendererFeature
{
	private class DewGBufferClearPass : ScriptableRenderPass
	{
		private static readonly ProfilingSampler Sampler = new ProfilingSampler("Dew Clear GBuffer Occlusion");

		public UniversalRenderer renderer;

		public Color clearColor;

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			RTHandle val = ((renderer != null) ? renderer.gbufferSpecularOcclusionAttachment : null);
			if (val != null)
			{
				CommandBuffer commandBuffer = CommandBufferPool.Get();
				ProfilingScope val2 = new ProfilingScope(commandBuffer, Sampler);
				try
				{
					CoreUtils.SetRenderTarget(commandBuffer, val, (ClearFlag)1, clearColor, 0, CubemapFace.Unknown, -1);
				}
				finally
				{
					((IDisposable)val2/*cast due to constrained. prefix*/).Dispose();
				}
				context.ExecuteCommandBuffer(commandBuffer);
				CommandBufferPool.Release(commandBuffer);
			}
		}

		static DewGBufferClearPass()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Expected Obj, but got Unknown
		}
	}

	[Tooltip("Alpha is the occlusion channel. 1 means 'not occluded', which is what shaders already substitute for pixels with no geometry. Leaving alpha at 0 reproduces the bug this feature exists to fix.")]
	public Color clearColor = new Color(0f, 0f, 0f, 1f);

	private DewGBufferClearPass _pass;

	public override void Create()
	{
		DewGBufferClearPass dewGBufferClearPass = new DewGBufferClearPass();
		((ScriptableRenderPass)dewGBufferClearPass).renderPassEvent = (RenderPassEvent)210;
		_pass = dewGBufferClearPass;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		UniversalRenderer val = (UniversalRenderer)(object)((renderer is UniversalRenderer) ? renderer : null);
		if (val != null)
		{
			_pass.renderer = val;
			_pass.clearColor = clearColor;
			renderer.EnqueuePass((ScriptableRenderPass)(object)_pass);
		}
	}
}
