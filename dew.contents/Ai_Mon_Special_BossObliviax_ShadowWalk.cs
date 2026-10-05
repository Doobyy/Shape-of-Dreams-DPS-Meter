using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossObliviax_ShadowWalk : AbilityInstance
{
	public float appearDistance;

	public float startTime;

	public float disappearTime;

	public float appearTime;

	public float postDelay;

	public GameObject fxDisappear;

	public GameObject fxAppear;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			postDelay = ((Random.value <= 0.8f) ? 0f : postDelay);
			float duration = startTime + disappearTime + appearTime + postDelay;
			info.caster.Control.StartChannel(new Channel
			{
				blockedActions = Channel.BlockedAction.Everything,
				duration = duration
			});
			Entity target = info.target;
			Vector3 aIAgentPosition = target.GetAIAgentPosition(info.caster);
			Vector3 normalized = (aIAgentPosition - AbilityTrigger.PredictPoint_Simple(info.caster, 1f, info.caster, disappearTime + appearTime)).normalized;
			Vector3 dest = Dew.GetValidAgentDestination_LinearSweep(end: aIAgentPosition + normalized * appearDistance, start: info.caster.agentPosition);
			FxPlayNetworked(fxDisappear, info.caster);
			yield return new SI.WaitForSeconds(startTime);
			CreateStatusEffect(info.caster, (Se_HunterBuff_ShadowWalk_Disappear s) =>
			{
				s.duration = disappearTime;
			});
			yield return null;
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				destination = dest,
				canGoOverTerrain = true,
				duration = disappearTime,
				ease = DewEase.Linear,
				isCanceledByCC = false,
				isFriendly = true,
				rotateForward = false
			});
			normalized = (target.GetAIAgentPosition(info.caster) - dest).normalized;
			info.caster.Control.Rotate(normalized, immediately: true);
			yield return new SI.WaitForSeconds(disappearTime);
			FxPlayNetworked(fxAppear, info.caster);
			yield return new SI.WaitForSeconds(appearTime);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
