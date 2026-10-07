using UnityEngine;

public class FxParticleSystem : MonoBehaviour
{
	public enum ClearParticlesBehavior
	{
		Dont,
		ClearSelf,
		ClearWithChildren
	}

	public ClearParticlesBehavior clearParticlesOnStop;

	[Header("Attached Effects")]
	public bool dontPauseAttachedWhenTeleport;

	public bool dontPauseAttachedWhenRendererDisabled = true;

	public bool hideAttachedWhenRendererDisabled;

	[Header("Inside Entity Model")]
	public bool dontDisableAsPartOfEntityRenderer;
}
