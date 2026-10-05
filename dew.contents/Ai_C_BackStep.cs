using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_C_BackStep : AbilityInstance
{
	public GameObject[] points;

	public int bombCount;

	public float backStepDis;

	public float stepDuration;

	public DewEase ease;

	public float delay;

	public ScalingValue speedAmount;

	public float speedDuration = 3f;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			Vector3 end = info.caster.agentPosition - info.forward * backStepDis;
			end = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, end);
			info.caster.Control.StartDaze(stepDuration);
			CreateBasicEffect(info.caster, new UncollidableEffect(), stepDuration);
			info.caster.Visual.KnockUp(0.4f, isFriendly: true);
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = false,
				canGoOverTerrain = true,
				destination = end,
				duration = stepDuration,
				ease = ease,
				rotateForward = false,
				isFriendly = true
			});
			CreateBasicEffect(info.caster, new SpeedEffect
			{
				strength = GetValue(speedAmount)
			}, speedDuration + stepDuration);
			Vector3 startPos = info.caster.agentPosition;
			for (int i = 0; i < bombCount; i++)
			{
				Vector3 positionOnGround = Dew.GetPositionOnGround(points[i].transform.position);
				positionOnGround = Dew.GetValidAgentDestination_LinearSweep(startPos, positionOnGround);
				CreateAbilityInstance<Ai_C_BackStep_Bomb>(positionOnGround, null, new CastInfo(info.caster, positionOnGround));
				yield return new SI.WaitForSeconds(0.03f);
			}
			yield return new SI.WaitForSeconds(delay);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
