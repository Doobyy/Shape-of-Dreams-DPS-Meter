using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossMaw_Chomp_DashAtk : AbilityInstance
{
	public float delay;

	public float postDelay;

	public float dashDuration;

	public float atkDelay;

	public float healAmount;

	public DewCollider range;

	public DewCollider backRange;

	public DewEase dashEase;

	public ScalingValue dmgFactor;

	public Knockback knockback;

	public DewAnimationClip clip;

	public DewAnimationClip endClip;

	public GameObject fxHit;

	public GameObject fxChomp;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			info.caster.Control.StartDaze(delay + dashDuration + postDelay);
			yield return new SI.WaitForSeconds(delay);
			info.caster.Animation.PlayAbilityAnimation(clip);
			info.caster.CreateBasicEffect(info.caster, new InvulnerableEffect(), dashDuration, "invul_bossmawchomp");
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				destination = info.point,
				ease = dashEase,
				isCanceledByCC = false,
				isFriendly = true,
				rotateForward = true,
				rotateSmoothly = true,
				duration = dashDuration,
				onFinish = () =>
				{
					((MonoBehaviour)(object)this).StartCoroutine(Routine());
				}
			});
			yield return new SI.WaitForSeconds(0.2f);
			info.caster.Animation.PlayAbilityAnimation(endClip);
		}
		IEnumerator Routine()
		{
			FxPlayNetworked(fxChomp, info.caster);
			yield return new SI.WaitForSeconds(atkDelay);
			range.transform.position = info.caster.agentPosition;
			backRange.transform.position = info.caster.agentPosition;
			range.transform.rotation = info.caster.rotation;
			backRange.transform.rotation = info.caster.rotation * backRange.transform.localRotation;
			int num = 0;
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetDirection(info.forward).Dispatch(entity);
				knockback.ApplyWithDirection(info.forward, entity);
				FxPlayNewNetworked(fxHit, entity);
				num++;
			}
			List<Entity> entities2 = backRange.GetEntities(out var handle2, tvDefaultHarmfulEffectTargets);
			for (int j = 0; j < entities2.Count; j++)
			{
				if (!entities.Contains(entities2[j]))
				{
					Entity entity2 = entities2[j];
					CreateDamage(DamageData.SourceType.Default, dmgFactor).SetDirection(info.forward).Dispatch(entity2);
					knockback.ApplyWithOrigin(info.caster.agentPosition, entity2);
					FxPlayNewNetworked(fxHit, entity2);
				}
			}
			handle.Return();
			handle2.Return();
			Heal(info.caster.Status.missingHealth * healAmount * (float)num).Dispatch(info.caster);
			yield return new SI.WaitForSeconds(1f);
			DestroyIfActive();
		}
	}

	private void MirrorProcessed()
	{
	}
}
