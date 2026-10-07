using UnityEngine;

public class Ai_E_ShadowVolley_Projectile : StandardProjectile
{
	public ScalingValue damage;

	public override bool reuseInRoom => true;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		SetCustomStartPosition(info.caster.agentPosition.WithY(info.caster.Visual.GetBonePosition((HumanBodyBones)18).y) + info.forward * 1.15f);
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(damage).SetDirection(info.forward).SetElemental(ElementalType.Dark).Dispatch(hit.entity);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
