using DG.Tweening;
using Mirror;
using UnityEngine;

public class Mon_SnowMountain_Scavenger : Monster, ISpawnableAsMiniBoss
{
	public float throwChance = 0.5f;

	public float jumpRandomPositionMag = 3f;

	public float jumpBehindOfTargetDistance = 2f;

	public float jumpChance = 0.25f;

	public Transform weaponTransform;

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((Object)(object)context.targetEnemy == null))
		{
			if (Random.value < throwChance && AI.Helper_CanBeCast<At_Mon_SnowMountain_Scavenger_Throw>() && AI.Helper_IsTargetInRange<At_Mon_SnowMountain_Scavenger_Throw>() && !AI.Helper_IsTargetInRangeOfAttack())
			{
				AI.Helper_CastAbilityAuto<At_Mon_SnowMountain_Scavenger_Throw>();
			}
			else if (Random.value < jumpChance && !AI.Helper_IsTargetInRangeOfAttack() && AI.Helper_CanBeCast<At_Mon_SnowMountain_Scavenger_Jump>())
			{
				Vector3 vector = context.targetEnemy.position - position;
				Vector3 point = position + vector.normalized * (vector.magnitude + jumpBehindOfTargetDistance) + Random.insideUnitCircle.ToXZ() * jumpRandomPositionMag;
				AI.Helper_CastAbility<At_Mon_SnowMountain_Scavenger_Jump>(new CastInfo(this, point));
			}
			else
			{
				AI.Helper_ChaseTarget();
			}
		}
	}

	public void HideWeaponTemporarilyLocal()
	{
		ShortcutExtensions.DOKill((Component)weaponTransform, false);
		TweenSettingsExtensions.SetId<Sequence>(TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.Append(DOTween.Sequence(), (Tween)(object)ShortcutExtensions.DOScale(weaponTransform, Vector3.zero, 0.1f)), 0.75f), (Tween)(object)ShortcutExtensions.DOScale(weaponTransform, Vector3.one, 0.3f)), (object)weaponTransform);
	}

	protected override void OnDeath(EventInfoKill info)
	{
		base.OnDeath(info);
		ShortcutExtensions.DOKill((Component)weaponTransform, false);
		weaponTransform.localScale = Vector3.zero;
	}

	public void OnBeforeSpawnAsMiniBoss()
	{
	}

	public void OnCreateAsMiniBoss()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			ISpawnableAsMiniBoss.GiveGenericMiniBossBonus(this);
			Ability.GetAbility<At_Mon_SnowMountain_Scavenger_Jump>().configs[0].cooldownTime = 0.65f;
			jumpChance = 1f;
		}
	}

	private void MirrorProcessed()
	{
	}
}
