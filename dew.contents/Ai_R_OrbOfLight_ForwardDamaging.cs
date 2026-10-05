using UnityEngine;

public class Ai_R_OrbOfLight_ForwardDamaging : StandardProjectile
{
	public ScalingValue damage;

	private int _hitEnemies;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_hitEnemies = 0;
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		if (_hitEnemies > 0)
		{
			CreateAbilityInstance(position, Quaternion.identity, new CastInfo(info.caster, info.caster), (Ai_R_OrbOfLight_BackwardHealing ai) =>
			{
				ai.NetworkhitEnemies = _hitEnemies;
			});
		}
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(damage).SetElemental(ElementalType.Light).SetDirection(rotation).Dispatch(hit.entity);
		if (hit.entity.IsAnyBoss())
		{
			_hitEnemies += 3;
		}
		else
		{
			_hitEnemies++;
		}
	}

	private void MirrorProcessed()
	{
	}
}
