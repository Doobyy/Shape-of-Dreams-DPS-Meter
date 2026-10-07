using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_HunterBuff_ShadowWalk : AbilityInstance
{
	public float maxDistance;

	public float minDistance;

	public float randomMagnitude;

	public float startTime;

	public float disappearTime;

	public float appearTime;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			info.caster.Control.StartChannel(new Channel
			{
				blockedActions = Channel.BlockedAction.Everything,
				duration = startTime + disappearTime + appearTime
			});
			Vector3 vector = info.target.agentPosition - info.caster.agentPosition + Random.onUnitSphere.Flattened() * randomMagnitude;
			if (vector.magnitude > maxDistance)
			{
				vector = vector.normalized * maxDistance;
			}
			else if (vector.magnitude < minDistance)
			{
				vector = vector.normalized * minDistance;
			}
			Vector3 dest = Dew.GetValidAgentDestination_LinearSweep(end: info.caster.agentPosition + vector, start: info.caster.agentPosition);
			yield return new SI.WaitForSeconds(startTime);
			CreateStatusEffect(info.caster, (Se_HunterBuff_ShadowWalk_Disappear s) =>
			{
				s.duration = disappearTime;
			});
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				destination = dest,
				canGoOverTerrain = true,
				duration = disappearTime,
				ease = DewEase.Linear,
				isCanceledByCC = false,
				isFriendly = true
			});
			yield return new SI.WaitForSeconds(disappearTime);
			yield return new SI.WaitForSeconds(appearTime);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
