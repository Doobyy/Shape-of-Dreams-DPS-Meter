using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossObliviax_BackStep_Instance : AbilityInstance
{
	public DewCollider range;

	public ScalingValue dmgFactor;

	public Knockback Knockback;

	public float castDuration;

	public float postDelay;

	public GameObject fxInstance;

	public GameObject fxTelegraph;

	public GameObject fxHit;

	public DewAnimationClip startAnim;

	public DewAnimationClip endAnim;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			CreateBasicEffect(info.caster, new UnstoppableEffect(), 10f, "ObliviaxUnstoppable").DestroyOnDestroy(this);
			info.caster.Control.StartDaze(castDuration + postDelay);
			info.caster.Animation.PlayAbilityAnimation(startAnim);
			yield return new SI.WaitForSeconds(0.2f);
			FxPlayNetworked(fxTelegraph, info.caster);
			yield return new SI.WaitForSeconds(castDuration);
			FxStopNetworked(fxTelegraph);
			info.caster.Animation.PlayAbilityAnimation(endAnim);
			FxPlayNetworked(fxInstance, info.caster);
			range.transform.position = info.caster.position;
			range.transform.rotation = info.caster.rotation;
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				FxPlayNewNetworked(fxHit, entity);
				Knockback.ApplyWithOrigin(info.caster.position, entity);
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetDirection(info.forward).SetOriginPosition(info.caster.agentPosition).SetElemental(ElementalType.Dark)
					.Dispatch(entity);
				CreateBasicEffect(entity, new StunEffect(), 1.25f);
			}
			handle.Return();
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
