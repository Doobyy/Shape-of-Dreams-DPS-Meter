using UnityEngine;

public class At_Mon_Special_BossPolaris_Holy_GoldenSpear : AbilityTrigger
{
	public override void OnCastStart(int configIndex, CastInfo info)
	{
		base.OnCastStart(configIndex, info);
		Vector3 vector = info.point - owner.agentPosition;
		Vector3 vector2 = Vector3.Cross(Vector3.up, vector.normalized);
		Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, info.caster.agentPosition + vector2 * 13f);
		float num = Vector3.Distance(owner.agentPosition, validAgentDestination_LinearSweep);
		Vector3 vector3 = Vector3.Cross(Vector3.down, vector.normalized);
		Vector3 validAgentDestination_LinearSweep2 = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, info.caster.agentPosition + vector3 * 13f);
		float num2 = Vector3.Distance(owner.agentPosition, validAgentDestination_LinearSweep2);
		Vector3 destination = ((num * Random.Range(0.9f, 1.1f) > num2 * Random.Range(0.9f, 1.1f)) ? validAgentDestination_LinearSweep : validAgentDestination_LinearSweep2);
		owner.Control.StartDisplacement(new DispByDestination
		{
			destination = destination,
			affectedByMovementSpeed = false,
			canGoOverTerrain = true,
			curve = new Vector4(0f, 1.1f, 1f, 0.34f),
			duration = 2.25f,
			ease = DewEase.EaseInOutQuad,
			isFriendly = true,
			rotateForward = false
		});
	}

	private void MirrorProcessed()
	{
	}
}
