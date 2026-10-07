using UnityEngine;

public class Ai_Mon_LavaLand_Magmadon_Charge_Projectile : StandardProjectile
{
	public ScalingValue dmgFactor;

	public DewCollider range;

	public Knockback Knockback;

	public GameObject fxExplode;

	public override bool reuseInRoom => true;

	protected override void OnComplete()
	{
		base.OnComplete();
		FxPlayNewNetworked(fxExplode, info.point, Quaternion.identity);
		range.transform.position = info.point;
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			if (!entity.IsNullInactiveDeadOrKnockedOut())
			{
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.point).SetElemental(ElementalType.Fire).Dispatch(entity);
				Knockback.ApplyWithOrigin(info.point, entity);
			}
		}
		handle.Return();
		CreateAbilityInstance<Ai_Mon_LavaLAnd_Magmadon_Charge_Magma>(info.point, Quaternion.identity, info);
	}

	private void MirrorProcessed()
	{
	}
}
