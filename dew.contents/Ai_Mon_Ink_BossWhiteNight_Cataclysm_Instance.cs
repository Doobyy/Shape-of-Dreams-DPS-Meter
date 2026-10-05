using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossWhiteNight_Cataclysm_Instance : AbilityInstance
{
	public float ignoreTime;

	public float delay;

	public float stunDuration;

	public GameObject fxAtk;

	public GameObject fxHit;

	public DewAnimationClip atkClip;

	public ScalingValue dmgFactor;

	public Knockback Knockback;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		yield return new SI.WaitForSeconds(delay);
		info.caster.Animation.PlayAbilityAnimation(atkClip);
		FxPlayNetworked(fxAtk, info.caster);
		yield return new SI.WaitForSeconds(0.05f);
		foreach (Entity item in new List<Entity>(NetworkedManagerBase<ActorManager>.instance.allEntities))
		{
			if (!item.IsNullInactiveDeadOrKnockedOut() && item.Status.TryGetStatusEffect<Se_Mon_Ink_BossWhiteNight_Cataclysm_NotSafe>(out var _))
			{
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.caster.position).Dispatch(item);
				Knockback.ApplyWithOrigin(info.caster.position, item);
				FxPlayNewNetworked(fxHit, item);
				if (!item.Status.hasCrowdControlImmunity)
				{
					CreateBasicEffect(item, new StunEffect(), stunDuration);
				}
			}
		}
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (allEntity.Status.TryGetStatusEffect<Se_Mon_Ink_BossWhiteNight_Cataclysm_NotSafe>(out var effect))
			{
				effect.Destroy();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
