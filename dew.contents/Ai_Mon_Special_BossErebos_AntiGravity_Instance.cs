using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossErebos_AntiGravity_Instance : AbilityInstance
{
	private class Ad_CheckFloat
	{
	}

	public float startDelay;

	public float radius;

	public float upDuration;

	public float floatDuration;

	public float downDuration;

	public float endStunDuration;

	public ScalingValue dmgFactor;

	public DewAnimationClip floatClip;

	public GameObject fxStart;

	public GameObject fxFloat;

	public GameObject fxEntity;

	public GameObject fxEnd;

	public GameObject fxHit;

	public GameObject fxTelegraph;

	private float _descendTimeSync;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		float totalDuration = downDuration + upDuration + floatDuration;
		FxPlayNetworked(fxTelegraph, info.point, Quaternion.identity);
		yield return new SI.WaitForSeconds(startDelay);
		_descendTimeSync = Time.time + floatDuration + upDuration;
		FxPlayNetworked(fxStart, info.point, Quaternion.identity);
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.point, radius, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < list.Count; i++)
		{
			Entity entity = list[i];
			if (!entity.Status.hasCrowdControlImmunity && !entity.HasData<Ad_CheckFloat>())
			{
				CreateBasicEffect(entity, new StunEffect(), totalDuration, "antigravity_stun");
				FxPlayNewNetworked(fxFloat, entity);
				FxPlayNewNetworked(fxEntity, entity);
				entity.Animation.PlayAbilityAnimation(floatClip);
				entity.AddData(new Ad_CheckFloat());
			}
		}
		handle.Return();
		yield return new SI.WaitForCondition(() => _descendTimeSync - Time.time <= 0.001f);
		FxStopNetworked(fxStart);
		FxPlayNetworked(fxEnd, info.point, Quaternion.identity);
		yield return new SI.WaitForSeconds(downDuration);
		list = DewPhysics.OverlapCircleAllEntities(out var handle2, info.point, radius, tvDefaultHarmfulEffectTargets);
		for (int num = 0; num < list.Count; num++)
		{
			Entity entity2 = list[num];
			FxPlayNewNetworked(fxHit, entity2);
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.point).Dispatch(entity2);
			if (!entity2.Status.hasCrowdControlImmunity)
			{
				entity2.Visual.KnockUp(KnockUpStrength.Big, isFriendly: false);
				CreateBasicEffect(entity2, new StunEffect(), endStunDuration, "antigravity_stun");
			}
		}
		handle2.Return();
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		FxStopNetworked(fxStart);
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (allEntity.HasData<Ad_CheckFloat>())
			{
				allEntity.RemoveData<Ad_CheckFloat>();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
