using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_SnowMountain_BossSkoll_AuraSlice : AbilityInstance
{
	public float delay;

	public float postDelay;

	public ScalingValue dmgFactor;

	public DewCollider range;

	public Knockback knockback;

	public DewAnimationClip clip;

	public GameObject fxHit;

	public GameObject fxTelegraph;

	public GameObject fxInstance;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
			info.caster.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
			Quaternion rot = Quaternion.Euler(0f, Random.Range(45f, -45f), 0f);
			rot = rotation * rot;
			FxPlayNetworked(fxTelegraph, info.point, rot);
			yield return new SI.WaitForSeconds(delay);
			info.caster.Animation.PlayAbilityAnimation(clip);
			FxPlayNetworked(fxInstance, info.point, rot);
			range.transform.position = info.point;
			range.transform.rotation = rot;
			Vector3 normalized = (info.point - info.caster.agentPosition).normalized;
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.point).SetDirection(normalized).SetElemental(ElementalType.Cold)
					.Dispatch(entity);
				FxPlayNewNetworked(fxHit, entity);
				knockback.ApplyWithDirection(normalized, entity);
			}
			handle.Return();
			yield return new SI.WaitForSeconds(postDelay);
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			info.caster.Control.DecrementBlockCounters(Channel.BlockedAction.Everything);
		}
	}

	private void MirrorProcessed()
	{
	}
}
