using DG.Tweening;
using UnityEngine;

public class Mon_SnowMountain_VikingWarrior : Monster
{
	public Transform shieldTransform;

	public Transform helmetTransform;

	public override void OnStartServer()
	{
		base.OnStartServer();
		CreateStatusEffect<Se_Mon_SnowMountain_VikingWarrior_DmgReducer>(this, new CastInfo(this));
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((Object)(object)context.targetEnemy == null))
		{
			if (AI.Helper_IsTargetInRange<At_Mon_SnowMountain_VikingWarrior_SpawnSword>() && AI.Helper_CanBeCast<At_Mon_SnowMountain_VikingWarrior_SpawnSword>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_SnowMountain_VikingWarrior_SpawnSword>();
			}
			else
			{
				AI.Helper_ChaseTarget();
			}
		}
	}

	protected override void OnDeath(EventInfoKill info)
	{
		base.OnDeath(info);
		ShortcutExtensions.DOKill((Component)shieldTransform, false);
		shieldTransform.localScale = Vector3.zero;
		ShortcutExtensions.DOKill((Component)helmetTransform, false);
		helmetTransform.localScale = Vector3.zero;
	}

	public void HideWeaponTemporarilyLocal()
	{
		ShortcutExtensions.DOKill((Component)shieldTransform, false);
		TweenSettingsExtensions.SetId<Sequence>(TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.Append(DOTween.Sequence(), (Tween)(object)ShortcutExtensions.DOScale(shieldTransform, Vector3.zero, 0.1f)), 0.75f), (Tween)(object)ShortcutExtensions.DOScale(shieldTransform, Vector3.one, 0.3f)), (object)shieldTransform);
		ShortcutExtensions.DOKill((Component)helmetTransform, false);
		TweenSettingsExtensions.SetId<Sequence>(TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.Append(DOTween.Sequence(), (Tween)(object)ShortcutExtensions.DOScale(helmetTransform, Vector3.zero, 0.1f)), 0.75f), (Tween)(object)ShortcutExtensions.DOScale(helmetTransform, Vector3.one, 0.3f)), (object)helmetTransform);
	}

	private void MirrorProcessed()
	{
	}
}
