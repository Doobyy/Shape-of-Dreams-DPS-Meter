using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossLightElemental_BeamBarrage : AbilityInstance
{
	[Serializable]
	public struct Phase
	{
		public float phaseTelegraphDelay;

		public Vector2[] beamAngles;

		public float duration;

		public float postDelay;
	}

	public Phase[] phases;

	public float postDelay;

	public GameObject beamTelegraphEffect;

	public GameObject telegraphingEffect;

	public GameObject beamingEffect;

	public GameObject beamingStopEffect;

	public DewAnimationClip animTelegraph;

	public DewAnimationClip animBeaming;

	public DewAnimationClip animBeamingDone;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		float num = postDelay;
		for (int i = 0; i < phases.Length; i++)
		{
			num += phases[i].duration + phases[i].postDelay + phases[i].phaseTelegraphDelay;
		}
		info.caster.Control.StartDaze(num);
		for (int j = 0; j < phases.Length; j++)
		{
			if (animTelegraph != null)
			{
				info.caster.Animation.PlayAbilityAnimation(animTelegraph);
			}
			FxPlayNetworked(telegraphingEffect);
			Phase phase = phases[j];
			for (int k = 0; k < phase.beamAngles.Length; k++)
			{
				Vector2 vector = Vector2.one * info.angle + phase.beamAngles[k];
				FxPlayNewNetworked(beamTelegraphEffect, info.caster.position, Quaternion.Euler(0f, vector.x, 0f));
			}
			yield return new SI.WaitForSeconds(phase.phaseTelegraphDelay);
			if (animBeaming != null)
			{
				info.caster.Animation.PlayAbilityAnimation(animBeaming);
			}
			FxStopNetworked(telegraphingEffect);
			FxPlayNetworked(beamingEffect);
			for (int l = 0; l < phase.beamAngles.Length; l++)
			{
				Vector2 beamAngle = Vector2.one * info.angle + phase.beamAngles[l];
				CreateAbilityInstance(info.caster.position, null, new CastInfo(info.caster), (Ai_Mon_Special_BossLightElemental_Beam b) =>
				{
					b.Networkangle = beamAngle;
					b.Networkduration = phase.duration;
				});
			}
			yield return new SI.WaitForSeconds(phase.duration);
			FxStopNetworked(beamingEffect);
			FxPlayNetworked(beamingStopEffect);
			if (animBeamingDone != null)
			{
				info.caster.Animation.PlayAbilityAnimation(animBeamingDone);
			}
			yield return new SI.WaitForSeconds(phase.postDelay);
		}
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		FxStop(beamingEffect);
		FxStop(telegraphingEffect);
	}

	private void MirrorProcessed()
	{
	}
}
