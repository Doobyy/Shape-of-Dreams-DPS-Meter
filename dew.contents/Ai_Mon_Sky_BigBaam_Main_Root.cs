using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_BigBaam_Main_Root : AbilityInstance
{
	public DewCollider range;

	public bool destroyOnCasterDeath;

	public float checkInterval;

	public float slowDelay;

	public float explodeDelay;

	public ScalingValue explodeDamage;

	public GameObject explodeEffect;

	public GameObject explodeHitEffect;

	public bool immediatelyAttackOnHit;

	private float _lastCheckInterval = float.NegativeInfinity;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_lastCheckInterval = float.NegativeInfinity;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		position = info.point;
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		if (destroyOnCasterDeath)
		{
			DestroyOnDeath(info.caster);
		}
		yield return new SI.WaitForSeconds(explodeDelay);
		FxPlayNetworked(explodeEffect);
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			Damage(explodeDamage).SetElemental(ElementalType.Light).SetOriginPosition(position).Dispatch(entity);
			if (entity.Status.TryGetStatusEffect<Se_Mon_Sky_BigBaam_Main_Root_Slowed>(out var effect))
			{
				effect.Destroy();
			}
			CreateStatusEffect<Se_Mon_Sky_BigBaam_Main_Root_Rooted>(entity);
			FxPlayNewNetworked(explodeHitEffect, entity);
		}
		if (immediatelyAttackOnHit && entities.Count > 0 && !info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			Entity target = entities[Random.Range(0, entities.Count)];
			ResetCooldown(info.caster.Ability.attackAbility);
			info.caster.Control.Attack(target, doChase: true);
		}
		handle.Return();
		Destroy();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || Time.time - _lastCheckInterval < checkInterval || Time.time - creationTime < slowDelay)
		{
			return;
		}
		_lastCheckInterval = Time.time;
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			if (entity.Status.TryGetStatusEffect<Se_Mon_Sky_BigBaam_Main_Root_Slowed>(out var effect))
			{
				effect.ResetTimer();
			}
			else
			{
				CreateStatusEffect<Se_Mon_Sky_BigBaam_Main_Root_Slowed>(entity);
			}
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
