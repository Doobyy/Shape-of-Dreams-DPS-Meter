using System;

public class Ai_R_QuickTrigger_Projectile : StandardProjectile
{
	public ScalingValue dmg;

	public int maxHitCount = 3;

	[NonSerialized]
	public bool doAttackEffect;

	private int _currentHitCount;

	public override bool reuseInRoom => true;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		SetCustomStartPosition(Ai_Atk_LacertaRifle.GetLacertaMuzzlePosition(info.caster, info.rotation));
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		DamageData damageData = Damage(dmg).SetDirection(info.rotation);
		if (doAttackEffect)
		{
			damageData.DoAttackEffect(AttackEffectType.Others);
		}
		damageData.Dispatch(hit.entity);
		_currentHitCount++;
		if (_currentHitCount >= maxHitCount)
		{
			Destroy();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_currentHitCount = 0;
		doAttackEffect = false;
	}

	private void MirrorProcessed()
	{
	}
}
