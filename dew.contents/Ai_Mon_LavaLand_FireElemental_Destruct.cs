using Mirror;
using UnityEngine;

public class Ai_Mon_LavaLand_FireElemental_Destruct : AbilityInstance
{
	public GameObject tpStartEffect;

	public GameObject tpEndEffect;

	public float backPosDis;

	public float finishDelay = 2f;

	public float appearDuration = 1f;

	public float shieldMissingHpRatio = 0.4f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			GiveShield(info.caster, shieldMissingHpRatio * info.caster.Status.missingHealth, float.PositiveInfinity);
			FxPlayNetworked(tpStartEffect, info.caster);
			Vector3 end = info.target.agentPosition + (info.target.agentPosition - info.caster.agentPosition).normalized * backPosDis;
			Vector3 dest = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, end);
			info.caster.Visual.DisableRenderers();
			info.caster.Control.StartDaze(appearDuration);
			CreateBasicEffect(info.caster, new UntargetableEffect(), appearDuration);
			CreateBasicEffect(info.caster, new InvisibleEffect
			{
				ignoreReveal = true
			}, appearDuration);
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = true,
				canGoOverTerrain = true,
				destination = dest,
				duration = appearDuration,
				ease = DewEase.EaseOutCubic,
				isCanceledByCC = false,
				isFriendly = true,
				onFinish = () =>
				{
					info.caster.Control.StartDaze(finishDelay);
					FxPlayNetworked(tpEndEffect, info.caster);
					Vector3 positionOnGround = Dew.GetPositionOnGround(dest);
					CreateAbilityInstance<Ai_Mon_LavaLand_FireElemental_DestructSub>(positionOnGround, null, new CastInfo(info.caster));
					Destroy();
				},
				onCancel = Destroy,
				rotateForward = true
			});
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)info.caster != null)
		{
			info.caster.Visual.EnableRenderers();
		}
	}

	private void MirrorProcessed()
	{
	}
}
