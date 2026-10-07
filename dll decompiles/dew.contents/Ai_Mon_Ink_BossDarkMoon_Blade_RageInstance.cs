using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_Blade_RageInstance : AbilityInstance
{
	public float startDelay;

	public float postDelay;

	public DewCollider range;

	public ScalingValue dmgFactor;

	public Knockback Knockback;

	public DewAnimationClip startClip;

	public DewAnimationClip endClip;

	public GameObject fxAtk;

	public GameObject fxTelegraph;

	public GameObject fxHit;

	public GameObject fxEnd;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			range.transform.position = ((Component)(object)this).transform.position;
			range.transform.rotation = ((Component)(object)this).transform.rotation;
			Vector3 dir = ((Component)(object)this).transform.forward;
			info.caster.Animation.PlayAbilityAnimation(startClip, 0.5f);
			FxPlayNetworked(fxTelegraph, position, rotation);
			info.caster.Control.StartDaze(startDelay + postDelay);
			yield return new SI.WaitForSeconds(startDelay);
			FxPlayNetworked(fxAtk, info.caster);
			info.caster.Animation.PlayAbilityAnimation(endClip);
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.caster.position).SetDirection(dir).Dispatch(entity);
				Knockback.ApplyWithDirection(dir, entity);
				FxPlayNewNetworked(fxHit, entity);
			}
			handle.Return();
			FxPlayNetworked(fxEnd, info.caster);
			yield return new SI.WaitForSeconds(postDelay);
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		info.caster.Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
