using Mirror;
using UnityEngine;

public class Ai_U_BigChomp : InstantDamageInstance, IOtherPlayersTonedDownDisable
{
	public ScalingValue healPerHitAmount;

	public ScalingValue shieldPerHitAmount;

	public float duration = 3f;

	public float anyBossMultiplier = 2.5f;

	public float allyDamageReduction = 0.75f;

	public float reduceCooldown = 1f;

	public DewAnimationClip animBite;

	public GameObject fxGetBuff;

	private int _normalHitCount;

	private int _bossHitCount;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Animation.PlayAbilityAnimation(animBite);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_normalHitCount = 0;
		_bossHitCount = 0;
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		if (!info.caster.CheckEnemyOrNeutral(target) && !dmg.IsAmountModifiedBy(this))
		{
			dmg.SetAmountModifiedBy(this);
			dmg.ApplyReduction(allyDamageReduction);
		}
	}

	protected override void OnHit(Entity entity)
	{
		if (!entity.Status.hasDamageImmunity && !entity.IsNullInactiveDeadOrKnockedOut())
		{
			if (entity.IsAnyBoss())
			{
				_bossHitCount++;
			}
			else
			{
				_normalHitCount++;
			}
		}
		base.OnHit(entity);
	}

	protected override void OnAfterDelay()
	{
		base.OnAfterDelay();
		float num = (float)_normalHitCount + (float)_bossHitCount * (1f + anyBossMultiplier);
		if (!(num <= 0.0001f))
		{
			FxPlayNetworked(fxGetBuff, info.caster);
			Heal(GetValue(healPerHitAmount) * num).Dispatch(info.caster);
			GiveShield(info.caster, num * GetValue(shieldPerHitAmount), duration);
			ApplyCooldownReduction(firstTrigger, num * reduceCooldown);
		}
	}

	private void MirrorProcessed()
	{
	}
}
