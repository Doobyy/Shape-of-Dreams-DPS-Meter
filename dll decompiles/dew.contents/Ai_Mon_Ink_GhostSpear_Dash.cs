using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_GhostSpear_Dash : AbilityInstance
{
	public float dashDuration;

	public float dashDistance;

	public DewEase ease;

	public GameObject finishEffect;

	public float cooldownResetChance = 0.2f;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		if (Random.value <= cooldownResetChance)
		{
			ResetCooldown(firstTrigger);
		}
		Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, info.caster.agentPosition + info.forward * dashDistance);
		info.caster.Control.StartDaze(dashDuration);
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			isFriendly = true,
			affectedByMovementSpeed = false,
			canGoOverTerrain = false,
			destination = validAgentDestination_LinearSweep,
			duration = dashDuration,
			ease = ease,
			onCancel = () =>
			{
				Destroy();
			},
			onFinish = () =>
			{
				AbilityTrigger attackAbility = info.caster.Ability.attackAbility;
				if ((Object)(object)attackAbility != null)
				{
					ResetCooldown(attackAbility);
				}
				FxPlayNetworked(finishEffect);
				Destroy();
			},
			rotateForward = true
		});
	}

	private void MirrorProcessed()
	{
	}
}
