using Mirror;
using UnityEngine;

public class Ai_Gem_L_SuppressedArcanum : AbilityInstance
{
	public GameObject fxHit;

	public DewCollider range;

	public ScalingValue damage;

	public ScalingValue shieldAmount;

	public float shieldDuration;

	public ScalingValue currentHpSacrificeRatio;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		float num = info.caster.currentHealth * GetValue(currentHpSacrificeRatio);
		if (num > info.caster.currentHealth - 1f)
		{
			num = info.caster.currentHealth - 1f;
		}
		if (num > 0f)
		{
			PureDamage(num, 0f).SetAttr(DamageAttribute.IgnoreShield).Dispatch(info.caster);
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle))
		{
			if (!((Object)(object)entity == (Object)(object)info.caster))
			{
				Hit(entity);
			}
		}
		handle.Return();
		Hit(info.caster);
		Destroy();
		void Hit(Entity e)
		{
			if (info.caster.CheckEnemyOrNeutral(e))
			{
				Damage(damage).SetElemental(ElementalType.Light).SetOriginPosition(info.caster.position).Dispatch(e);
				FxPlayNewNetworked(fxHit, e);
			}
			else
			{
				GiveShield(e, GetValue(shieldAmount), shieldDuration);
				FxPlayNewNetworked(fxHit, e);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
