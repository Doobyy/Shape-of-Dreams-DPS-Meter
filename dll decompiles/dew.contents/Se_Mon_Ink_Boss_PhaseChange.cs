using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Mon_Ink_Boss_PhaseChange : StatusEffect
{
	public bool doInvulnerable;

	public float knockbackDistance;

	public float knockbackDuration;

	[Space(15f)]
	public float startDuration;

	public float staggerDuration;

	public DewAnimationClip startClip;

	public DewAnimationClip loopClip;

	public DewAnimationClip endClip;

	public GameObject fxStagger;

	[Space(15f)]
	public DewCollider range;

	public bool ignoreUnstoppable = true;

	[Space(15f)]
	public float rageStartDelay;

	public float rageStartDuration;

	public DewAnimationClip rageClip;

	public DewAnimationClip rageEndClip;

	public GameObject fxRageStart;

	public GameObject fxRageEnd;

	[Space(15f)]
	public float postDelay;

	internal bool enableRageMode;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(victim);
		if (doInvulnerable)
		{
			DoInvulnerable();
		}
		info.caster.Animation.StopAbilityAnimation();
		info.caster.Control.Stop();
		info.caster.Control.CancelOngoingChannels();
		info.caster.Control.ClearActionQueue();
		float num = startDuration + staggerDuration + postDelay;
		if (enableRageMode)
		{
			num += rageStartDuration + rageStartDelay;
		}
		info.caster.Control.StartDaze(num + 1.5f);
		info.caster.Animation.PlayAbilityAnimation(startClip);
		FxPlayNetworked(fxStagger, info.caster);
		range.transform.position = info.caster.agentPosition;
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			if (ignoreUnstoppable || !entity.Status.hasCrowdControlImmunity)
			{
				Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(entity.position, entity.position + (entity.position - info.caster.position).normalized * knockbackDistance);
				validAgentDestination_LinearSweep = Dew.GetPositionOnGround(validAgentDestination_LinearSweep);
				entity.Control.StartDisplacement(new DispByDestination
				{
					affectedByMovementSpeed = false,
					canGoOverTerrain = false,
					destination = validAgentDestination_LinearSweep,
					duration = knockbackDuration,
					ease = DewEase.EaseOutQuad,
					isFriendly = false,
					rotateForward = false
				});
			}
		}
		handle.Return();
		yield return new SI.WaitForSeconds(startDuration);
		DewAnimationEntry[] entries = loopClip.entries;
		for (int j = 0; j < entries.Length; j++)
		{
			entries[j].duration = staggerDuration + 0.3f;
		}
		info.caster.Animation.PlayAbilityAnimation(loopClip);
		yield return new SI.WaitForSeconds(staggerDuration);
		if (enableRageMode)
		{
			if (info.caster.Status.TryGetStatusEffect<Se_Mon_Ink_BossDeathInterrupt>(out var effect))
			{
				effect.Destroy();
			}
			info.caster.Animation.PlayAbilityAnimation(endClip);
			yield return new SI.WaitForSeconds(rageStartDelay);
			FxPlayNetworked(fxRageStart, info.caster);
			info.caster.Animation.PlayAbilityAnimation(rageClip);
			yield return new SI.WaitForSeconds(rageStartDuration);
			FxStopNetworked(fxRageStart);
			FxPlayNetworked(fxRageEnd, info.caster);
			info.caster.Animation.PlayAbilityAnimation(rageEndClip);
			CreateStatusEffect<Se_Mon_Ink_Boss_Rage>(info.caster, info);
			range.transform.position = info.caster.agentPosition;
			List<Entity> entities2 = range.GetEntities(out var handle2, tvDefaultHarmfulEffectTargets);
			for (int k = 0; k < entities2.Count; k++)
			{
				Entity entity2 = entities2[k];
				if (ignoreUnstoppable || !entity2.Status.hasCrowdControlImmunity)
				{
					Vector3 validAgentDestination_LinearSweep2 = Dew.GetValidAgentDestination_LinearSweep(entity2.position, entity2.position + (entity2.position - info.caster.position).normalized * knockbackDistance);
					validAgentDestination_LinearSweep2 = Dew.GetPositionOnGround(validAgentDestination_LinearSweep2);
					entity2.Control.StartDisplacement(new DispByDestination
					{
						affectedByMovementSpeed = false,
						canGoOverTerrain = false,
						destination = validAgentDestination_LinearSweep2,
						duration = knockbackDuration,
						ease = DewEase.EaseOutQuad,
						isFriendly = false,
						rotateForward = false
					});
				}
			}
			handle2.Return();
			yield return new SI.WaitForSeconds(postDelay);
			Destroy();
		}
		else
		{
			info.caster.Animation.PlayAbilityAnimation(endClip);
			info.caster.Control.StartDaze(postDelay);
			yield return new SI.WaitForSeconds(postDelay);
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxRageStart);
		}
	}

	private void MirrorProcessed()
	{
	}
}
