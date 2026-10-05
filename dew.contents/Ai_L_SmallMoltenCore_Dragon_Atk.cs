using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_L_SmallMoltenCore_Dragon_Atk : StandardProjectile
{
	public ScalingValue damage;

	public float healMaxHpRatio = 0.05f;

	public float explosionRadius = 2.5f;

	public float empowerAmp = 0.5f;

	public float overrideRotationDuration = 1f;

	public DewAnimationClip startAnim;

	public GameObject fxFly;

	public GameObject fxEmpoweredFly;

	public GameObject fxExplosion;

	[NonSerialized]
	public bool empowered;

	private float _baseInitialSpeed;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseInitialSpeed = initialSpeed;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		initialSpeed = _baseInitialSpeed;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			if (info.target.IsNullInactiveDeadOrKnockedOut())
			{
				Destroy();
				yield break;
			}
			DestroyOnDeath(info.caster);
			info.caster.Control.RotateTowards(info.target, immediately: false, overrideRotationDuration);
			FxPlayNetworked(empowered ? fxEmpoweredFly : fxFly);
			firstEntity.Animation.PlayAbilityAnimation(startAnim);
			yield return new SI.WaitForSeconds(overrideRotationDuration);
			DestroyIfActive();
		}
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		if (hit.entity.IsNullInactiveDeadOrKnockedOut())
		{
			return;
		}
		Damage(damage).SetElemental(ElementalType.Fire).DoAttackEffect(AttackEffectType.BasicAttackMain).Dispatch(hit.entity, chain);
		if (empowered)
		{
			FxPlayNetworked(fxExplosion, hit.entity);
			ListReturnHandle<Entity> handle;
			foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, ((Component)(object)this).transform.position, explosionRadius, tvDefaultHarmfulEffectTargets))
			{
				if (!((UnityEngine.Object)(object)item == (UnityEngine.Object)(object)hit.entity))
				{
					FxPlayNewNetworked(effectOnEntity, item);
					Damage(damage).ApplyAmplification(empowerAmp).SetElemental(ElementalType.Fire).Dispatch(item, chain);
				}
			}
			handle.Return();
		}
		if (!firstEntity.IsNullOrInactive())
		{
			Heal(healMaxHpRatio * firstEntity.maxHealth).Dispatch(firstEntity, chain);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxEmpoweredFly);
			FxStopNetworked(fxFly);
		}
	}

	private void MirrorProcessed()
	{
	}
}
