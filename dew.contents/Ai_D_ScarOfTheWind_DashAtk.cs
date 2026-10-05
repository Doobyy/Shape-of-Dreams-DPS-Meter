using Mirror;
using UnityEngine;

public class Ai_D_ScarOfTheWind_DashAtk : MeleeAttackInstance
{
	public Dash dash;

	public ScalingValue bonusDamage;

	public ScalingValue healPerHit;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (!info.caster.Status.hasImmobility)
		{
			dash.ApplyByDirection(info.caster, ((Component)(object)this).transform.rotation * Vector3.forward, (DispByDestination d) =>
			{
				d.canGoOverTerrain = true;
			});
			CreateBasicEffect(info.caster, new UncollidableEffect(), 0.15f);
			CreateBasicEffect(info.caster, new UnstoppableEffect(), 0.25f);
		}
		info.caster.Ability.originalAttackAbility.SetChargeAll(0);
		info.caster.Control.Rotate(((Component)(object)this).transform.rotation, immediately: true, 0.75f);
	}

	public override void OnHit(Entity entity, bool isMain)
	{
		base.OnHit(entity, isMain);
		Damage(bonusDamage).SetDirection(((Component)(object)this).transform.rotation).SetElemental(ElementalType.Dark).Dispatch(entity);
		if (!entity.Status.hasDamageImmunity)
		{
			Heal(healPerHit).SetCanMerge().Dispatch(info.caster);
		}
	}

	private void MirrorProcessed()
	{
	}
}
